using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Controls.Primitives;
using PermissionScope.Core;
using PermissionScope.Windows;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Windows.Storage.Pickers;
using Windows.System;
using Microsoft.UI.Text;
using static PermissionScope.App.Localization;

namespace PermissionScope.App;

public sealed class MainWindow : Window
{
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(IntPtr window);
    private readonly Grid root = new();
    private readonly Grid body = new();
    private readonly InfoBar notice = new() { IsClosable = true, Margin = new(28, 0, 28, 12) };
    private readonly TextBox pathBox = new() { MinWidth = 220, HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly TextBox identityBox = new() { MinWidth = 240 };
    private readonly TextBox filterBox = new();
    private readonly ListView objectList = new() { SelectionMode = ListViewSelectionMode.Single, HorizontalContentAlignment = HorizontalAlignment.Stretch };
    private readonly StackPanel detail = new() { Spacing = 16 };
    private ScrollViewer? detailHost;
    private readonly TextBlock progressLabel = new() { TextWrapping = TextWrapping.Wrap };
    private readonly ProgressBar progressBar = new() { IsIndeterminate = true, Visibility = Visibility.Collapsed };
    private CancellationTokenSource? scanCancellation;
    private PermissionSnapshot? snapshot;
    private ResourceAccess? selectedResource;
    private Button? analyzeButton;
    private Button? cancelButton;
    private CheckBox? recursiveBox;
    private CheckBox? filesBox;
    private string activeTab = "Access";
    private string filterMode = "All";
    private int page;
    private TextBlock? pageLabel;
    private bool busy;
    private bool saved;
    private bool savePending;
    private bool compactNavigation;
    private string currentPage = "Home";
    private readonly Dictionary<string, Button> navigationButtons = [];
    private readonly Dictionary<FrameworkElement, Panel> inputHosts = [];
    private Grid? resultSplit;
    private Grid? resultLayout;
    private FrameworkElement? resultDetail;
    private FrameworkElement? resultList;

    public MainWindow()
    {
        Initialize();
        Closed += (_, _) => scanCancellation?.Cancel();
    }
    private void Initialize()
    {
        Localization.Initialize();
        Title = "PermissionScope";
        var scale = Math.Max(1, GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96d);
        var display = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
        var area = display.WorkArea;
        var width = Math.Min((int)(1380 * scale), area.Width - (int)(32 * scale));
        var height = Math.Min((int)(900 * scale), area.Height - (int)(32 * scale));
        AppWindow.MoveAndResize(new(area.X + (area.Width - width) / 2, area.Y + (area.Height - height) / 2, width, height));
        var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "PermissionScope.ico");
        if (File.Exists(iconPath)) AppWindow.SetIcon(iconPath);
        root.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new() { Height = GridLength.Auto });
        root.ColumnDefinitions.Add(new() { Width = new(228) });
        root.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
        root.Background = ThemeBrush("ApplicationPageBackgroundThemeBrush");
        root.Children.Add(body); Grid.SetColumn(body, 1);
        root.Children.Add(notice); Grid.SetColumn(notice, 1); Grid.SetRow(notice, 1);
        Content = root;
        ApplyAppearance();
        BuildNavigation();
        ConfigureInputs();
        ShowHome();
        AddShortcut(VirtualKey.O, () => { ShowAnalyze(); pathBox.Focus(FocusState.Programmatic); });
        AddShortcut(VirtualKey.K, () => { ShowResults(); filterBox.Focus(FocusState.Programmatic); });
        AddShortcut(VirtualKey.F, () => { ShowResults(); filterBox.Focus(FocusState.Programmatic); });
        AddShortcut(VirtualKey.R, () => Run(AnalyzeAsync));
        AddShortcut(VirtualKey.E, () => Run(ExportAsync));
        var escape = new KeyboardAccelerator { Key = VirtualKey.Escape };
        escape.Invoked += (_, e) => { scanCancellation?.Cancel(); e.Handled = true; };
        root.KeyboardAccelerators.Add(escape);
        root.SizeChanged += (_, _) => AdaptLayout();
        root.ActualThemeChanged += (_, _) => ApplyPalette();
    }
    public void SetPath(string path) { pathBox.Text = path; ShowAnalyze(); }
    private bool IsDemo => snapshot?.FixtureId == DemoFixture.Version;
    public void ShowDemo(string scene)
    {
        snapshot = DemoFixture.Create(); saved = false; page = 0; filterMode = "All";
        filterBox.Text = "";
        pathBox.Text = DemoFixture.Root;
        selectedResource = scene == "unknown" ? snapshot.Resources.Last() : snapshot.Resources.First();
        activeTab = scene switch { "access-path" => "Why", "technical" => "Technical", "simulation" => "Simulate", _ => "Access" };
        if (scene == "home") ShowHome();
        else if (scene == "analyze") ShowAnalyze();
        else if (scene == "compare") Run(CompareAsync);
        else ShowResults();
    }
    private static InfoBar DemoBanner() => new() { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Informational, Title = T("DemoTitle"), Message = T("DemoNote") };
    private static SolidColorBrush Brush(byte r, byte g, byte b) => new(global::Windows.UI.Color.FromArgb(255, r, g, b));
    private static bool HighContrast => new global::Windows.UI.ViewManagement.AccessibilitySettings().HighContrast;
    private static Brush SystemBrush(bool foreground) => new SolidColorBrush(new global::Windows.UI.ViewManagement.UISettings().GetColorValue(
        foreground ? global::Windows.UI.ViewManagement.UIColorType.Foreground : global::Windows.UI.ViewManagement.UIColorType.Background));
    private static Brush NavigationInk => HighContrast ? SystemBrush(true) : new SolidColorBrush(Colors.White);
    private static Brush ThemeBrush(string key) => Application.Current.Resources.TryGetValue(key, out var resource) && resource is Brush brush
        ? brush : Brush(100, 107, 118);
    private static TextBlock Text(string text, double size = 14, bool strong = false) => new()
    {
        Text = text,
        FontSize = size,
        FontFamily = new("Segoe UI Variable, Segoe UI"),
        TextWrapping = TextWrapping.Wrap,
        FontWeight = strong ? FontWeights.SemiBold : FontWeights.Normal,
        IsTextSelectionEnabled = true
    };
    private static TextBlock Mono(string text) { var block = Text(text, 12); block.FontFamily = new("Cascadia Mono, Consolas"); block.FlowDirection = FlowDirection.LeftToRight; return block; }
    private static Button Button(string text, Action action, bool accent = false)
    {
        var button = new Button { Content = text, MinHeight = 38, Padding = new(16, 9, 16, 9) };
        AutomationProperties.SetName(button, text);
        if (accent)
        {
            button.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            if (!new global::Windows.UI.ViewManagement.AccessibilitySettings().HighContrast)
            { button.Background = Brush(39, 100, 231); button.Foreground = new SolidColorBrush(Colors.White); }
        }
        button.Click += (_, _) => action();
        return button;
    }
    private static FlowPanel Row(params UIElement[] children) { var panel = new FlowPanel(); foreach (var child in children) panel.Children.Add(child); return panel; }
    private static Border Line() => new() { Height = 1, Background = HighContrast ? SystemBrush(true) : new SolidColorBrush(global::Windows.UI.Color.FromArgb(48, 100, 107, 118)), Margin = new(0, 4, 0, 4) };
    private void AddShortcut(VirtualKey key, Action action)
    {
        var shortcut = new KeyboardAccelerator { Key = key, Modifiers = VirtualKeyModifiers.Control };
        shortcut.Invoked += (_, e) => { action(); e.Handled = true; };
        root.KeyboardAccelerators.Add(shortcut);
    }
    private void ApplyAppearance()
    {
        root.RequestedTheme = Localization.Preferences.Appearance switch { "Dark" => ElementTheme.Dark, "Light" => ElementTheme.Light, _ => ElementTheme.Default };
        root.FlowDirection = IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        root.Language = Locale.StartsWith("qps-", StringComparison.Ordinal) ? "en-US" : Locale;
        ApplyPalette();
    }
    private void ApplyPalette()
    {
        if (HighContrast) { root.Background = SystemBrush(false); return; }
        var dark = root.ActualTheme == ElementTheme.Dark;
        root.Background = dark ? Brush(17, 19, 23) : Brush(247, 248, 250);
        var color = ((SolidColorBrush)root.Background).Color;
        AppWindow.TitleBar.BackgroundColor = color; AppWindow.TitleBar.ButtonBackgroundColor = color;
        AppWindow.TitleBar.ForegroundColor = dark ? Colors.White : Colors.Black;
        AppWindow.TitleBar.ButtonForegroundColor = dark ? Colors.White : Colors.Black;
        AppWindow.TitleBar.InactiveBackgroundColor = color;
    }
    private void SelectNavigation(string key)
    {
        currentPage = key;
        foreach (var entry in navigationButtons)
        {
            entry.Value.Background = !HighContrast && entry.Key == key ? Brush(38, 65, 109) : new SolidColorBrush(Colors.Transparent);
            entry.Value.BorderThickness = HighContrast && entry.Key == key ? new(2) : new(0);
            if (HighContrast) entry.Value.BorderBrush = SystemBrush(true);
        }
    }
    private void AdaptLayout()
    {
        var compact = root.ActualWidth < 1060;
        if (compact != compactNavigation) { compactNavigation = compact; root.ColumnDefinitions[0].Width = new(compact ? 76 : 228); BuildNavigation(); }
        if (resultSplit != null && resultList != null && resultDetail != null && currentPage == "Results")
        {
            var narrow = body.ActualWidth < 680;
            if (resultLayout != null) resultLayout.Height = Math.Max(narrow ? 900 : 480, body.ActualHeight - 40);
            resultSplit.ColumnDefinitions[0].Width = narrow ? new(1, GridUnitType.Star) : new(0.38, GridUnitType.Star);
            resultSplit.ColumnDefinitions[1].Width = narrow ? new(0) : new(0.62, GridUnitType.Star);
            resultSplit.ColumnDefinitions[0].MinWidth = narrow ? 0 : 220; resultSplit.ColumnDefinitions[1].MinWidth = narrow ? 0 : 300;
            resultSplit.RowDefinitions[0].Height = narrow ? new(0.35, GridUnitType.Star) : new(1, GridUnitType.Star);
            resultSplit.RowDefinitions[1].Height = narrow ? new(0.65, GridUnitType.Star) : new(0);
            Grid.SetColumn(resultDetail, narrow ? 0 : 1); Grid.SetRow(resultDetail, narrow ? 1 : 0);
        }
    }
    private void BuildNavigation()
    {
        var old = root.Children.OfType<Border>().FirstOrDefault(b => b.Tag as string == "navigation");
        if (old != null) root.Children.Remove(old);
        navigationButtons.Clear();
        var panel = new Grid { Padding = compactNavigation ? new(10, 24, 10, 16) : new(20, 28, 20, 20) };
        panel.RowDefinitions.Add(new() { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
        panel.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var logo = new Canvas { Width = 35, Height = 40, FlowDirection = FlowDirection.LeftToRight };
        var figure = new PathFigure { StartPoint = new(6, 34), IsClosed = false };
        foreach (var point in new[] { new global::Windows.Foundation.Point(6, 6), new(29, 6), new(29, 21), new(6, 21) }) figure.Segments.Add(new LineSegment { Point = point });
        var geometry = new PathGeometry(); geometry.Figures.Add(figure);
        var link = new Microsoft.UI.Xaml.Shapes.Path { Data = geometry, Stroke = Brush(130, 166, 246), StrokeThickness = 2.5 };
        logo.Children.Add(link);
        foreach (var point in new[] { (6d, 34d), (6d, 6d), (29d, 6d), (29d, 21d), (6d, 21d) })
        { var node = new Ellipse { Width = 7, Height = 7, Fill = new SolidColorBrush(Colors.White) }; Canvas.SetLeft(node, point.Item1 - 3.5); Canvas.SetTop(node, point.Item2 - 3.5); logo.Children.Add(node); }
        var brand = Text("Permission\nScope", 21, true); brand.Foreground = NavigationInk; brand.LineHeight = 24;
        panel.Children.Add(compactNavigation ? logo : Row(logo, brand));
        var nav = new StackPanel { Spacing = 8, Margin = new(0, 46, 0, 0) };
        (string Key, string Glyph, Action Action)[] items = [("Home", "\uE80F", ShowHome), ("Analyze", "\uE8B7", ShowAnalyze), ("Scans", "\uE81C", () => Run(ShowSnapshotsAsync)), ("Compare", "\uE8AB", () => Run(CompareAsync)), ("Settings", "\uE713", ShowSettings)];
        foreach (var item in items)
        {
            var label = Text(T(item.Key), 14); label.Foreground = NavigationInk; label.IsTextSelectionEnabled = false;
            var icon = new FontIcon { Glyph = item.Glyph, FontSize = 17, Foreground = NavigationInk, VerticalAlignment = VerticalAlignment.Center };
            var button = Button(T(item.Key), item.Action);
            if (compactNavigation) { button.Content = icon; button.Padding = new(12); }
            else
            {
                var navContent = new Grid { ColumnSpacing = 12 };
                navContent.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
                navContent.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                navContent.Children.Add(icon); navContent.Children.Add(label); Grid.SetColumn(label, 1);
                button.Content = navContent;
            }
            AutomationProperties.SetAutomationId(button, "Nav" + item.Key);
            ToolTipService.SetToolTip(button, T(item.Key));
            button.HorizontalAlignment = HorizontalAlignment.Stretch; button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            button.Background = new SolidColorBrush(Colors.Transparent); button.BorderThickness = new(0);
            nav.Children.Add(button);
            navigationButtons[item.Key] = button;
        }
        var navigationScroll = new ScrollViewer { Content = nav, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        panel.Children.Add(navigationScroll); Grid.SetRow(navigationScroll, 1);
        var footer = new StackPanel { Spacing = 12 };
        var local = Text(T("LocalOnly"), 10, true); local.Foreground = HighContrast ? SystemBrush(true) : Brush(190, 207, 238);
        var free = Text(T("FreeForever") + "  /  1.0.0", 12); free.Foreground = HighContrast ? SystemBrush(true) : Brush(190, 207, 238);
        if (!compactNavigation) { footer.Children.Add(local); footer.Children.Add(free); }
        panel.Children.Add(footer); Grid.SetRow(footer, 2);
        var border = new Border { Child = panel, Background = HighContrast ? SystemBrush(false) : Brush(23, 49, 92), Tag = "navigation" };
        root.Children.Add(border); Grid.SetRowSpan(border, 2);
        var route = currentPage; SelectNavigation(route == "Results" ? "Analyze" : route); currentPage = route;
    }
    private void ConfigureInputs()
    {
        pathBox.Header = T("Path"); pathBox.PlaceholderText = "C:\\  ·  \\\\server\\share";
        pathBox.FlowDirection = FlowDirection.LeftToRight;
        identityBox.Header = T("Identity"); identityBox.PlaceholderText = T("IdentityHint");
        identityBox.FlowDirection = FlowDirection.LeftToRight;
        pathBox.AllowDrop = true;
        pathBox.DragOver += (_, e) => e.AcceptedOperation = DataPackageOperation.Copy;
        pathBox.Drop += (_, e) => Run(async () =>
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            { var items = await e.DataView.GetStorageItemsAsync(); if (items.Count > 0) pathBox.Text = items[0].Path; }
        });
        pathBox.KeyDown += (_, e) => { if (e.Key == VirtualKey.Enter) { e.Handled = true; Run(AnalyzeAsync); } };
        filterBox.PlaceholderText = T("Filter");
        AutomationProperties.SetAutomationId(pathBox, "AnalysisPath");
        AutomationProperties.SetAutomationId(identityBox, "AnalysisIdentity");
        AutomationProperties.SetName(filterBox, T("Filter"));
        filterBox.TextChanged += (_, _) => { page = 0; RefreshObjects(); };
        objectList.SelectionChanged += (_, _) =>
        {
            if (objectList.SelectedItem is ListViewItem { Tag: ResourceAccess resource }) { selectedResource = resource; RenderDetail(); }
        };
    }
    private StackPanel Page(string title, string? subtitle = null)
    {
        body.Children.Clear();
        var panel = new StackPanel { Spacing = 20, Margin = new(40, 32, 40, 32), MaxWidth = 1080, HorizontalAlignment = HorizontalAlignment.Stretch };
        panel.Children.Add(Text(title, 32, true));
        if (subtitle != null) panel.Children.Add(Text(subtitle, 15));
        if (IsDemo) panel.Children.Add(DemoBanner());
        body.Children.Add(new ScrollViewer { Content = panel, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        return panel;
    }
    private void ShowHome()
    {
        SelectNavigation("Home");
        var panel = Page("");
        panel.Children.Clear();
        panel.Margin = new(40, 48, 32, 32);
        var eyebrow = Text(T("Category"), 11, true); eyebrow.CharacterSpacing = 150; panel.Children.Add(eyebrow);
        var title = Text(T("HomeTitle"), 44, true); title.LineHeight = 51; title.CharacterSpacing = -20; panel.Children.Add(title);
        var intro = Text(T("HomeIntro"), 18); intro.MaxWidth = 570; intro.HorizontalAlignment = HorizontalAlignment.Left; intro.Margin = new(0, 0, 0, 16); panel.Children.Add(intro);
        var primary = Button(T("AnalyzeFolder") + "  →", ShowAnalyze, true); primary.MinHeight = 46; panel.Children.Add(primary);
        panel.Children.Add(Row(Button(T("FindUser"), () => { ShowAnalyze(); identityBox.Focus(FocusState.Programmatic); }), Button(T("OpenScan"), () => Run(ShowSnapshotsAsync))));
        panel.Children.Add(Button(T("TryDemo"), () => ShowDemo("access")));
        if (IsDemo) panel.Children.Add(DemoBanner());
        panel.Children.Add(new Border { Height = 40 });
        panel.Children.Add(Line());
        panel.Children.Add(Text(T("ReadOnlyNote"), 14));
        panel.Children.Add(Text(T("Privacy"), 12));
    }
    private void DetachInputs()
    {
        // FrameworkElement.Parent can be null after a page leaves the visual tree,
        // while its old panel still owns the control. Retain the actual owners.
        foreach (var entry in inputHosts) entry.Value.Children.Remove(entry.Key);
        inputHosts.Clear();
    }
    private void AddInput(Panel host, FrameworkElement input)
    {
        host.Children.Add(input);
        inputHosts.Add(input, host);
    }
    private void ShowAnalyze()
    {
        SelectNavigation("Analyze");
        DetachInputs();
        var panel = Page(T("AnalyzeTitle"), T("AnalyzeIntro"));
        var pathRow = new Grid { ColumnSpacing = 12 };
        pathRow.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) }); pathRow.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        AddInput(pathRow, pathBox);
        var browse = Button(T("Browse"), () => Run(BrowseAsync)); browse.VerticalAlignment = VerticalAlignment.Bottom; pathRow.Children.Add(browse); Grid.SetColumn(browse, 1);
        panel.Children.Add(pathRow);
        AddInput(panel, identityBox);
        panel.Children.Add(Text(T("IdentitySimpleNote"), 12));
        recursiveBox = new() { Content = T("Recursive"), IsChecked = recursiveBox?.IsChecked ?? true };
        filesBox = new() { Content = T("IncludeFiles"), IsChecked = filesBox?.IsChecked ?? false };
        panel.Children.Add(Row(recursiveBox, filesBox));
        analyzeButton = Button(T("Analyze"), () => Run(AnalyzeAsync), true); analyzeButton.IsEnabled = !busy;
        AutomationProperties.SetAutomationId(analyzeButton, "RunAnalysis");
        cancelButton = Button(T("Cancel"), () => scanCancellation?.Cancel());
        cancelButton.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        panel.Children.Add(Row(analyzeButton, cancelButton));
        if (!busy) progressLabel.Text = "";
        AddInput(panel, progressBar); AddInput(panel, progressLabel);
        panel.Children.Add(Line()); panel.Children.Add(Text(T("ReadOnlyNote"), 13));
    }
    private async Task BrowseAsync()
    {
        var picker = new FolderPicker(AppWindow.Id);
        var folder = await picker.PickSingleFolderAsync(); if (folder != null) pathBox.Text = folder.Path;
    }
    private async Task AnalyzeAsync()
    {
        if (busy) return;
        if (IsDemo && pathBox.Text == DemoFixture.Root) { ShowDemo("access"); return; }
        if (string.IsNullOrWhiteSpace(pathBox.Text)) { ShowAnalyze(); pathBox.Focus(FocusState.Programmatic); return; }
        var options = new ScanOptions(pathBox.Text, string.IsNullOrWhiteSpace(identityBox.Text) ? null : identityBox.Text.Trim(), recursiveBox?.IsChecked != false, filesBox?.IsChecked == true);
        busy = true; saved = false;
        scanCancellation?.Dispose(); scanCancellation = new();
        ShowAnalyze(); if (analyzeButton != null) analyzeButton.IsEnabled = false;
        progressBar.Visibility = Visibility.Visible; progressLabel.Text = T("Running");
        var last = Stopwatch.StartNew();
        var progress = new Progress<ScanProgress>(p =>
        {
            if (last.ElapsedMilliseconds < 100) return;
            last.Restart(); progressLabel.Text = $"{p.Analyzed:N0} {T("Objects")} · {p.Errors:N0} {T("Errors")} · {p.Elapsed:mm\\:ss}\n{p.Path}";
        });
        try
        {
            snapshot = await ScanWorker.ScanAsync(options, progress, scanCancellation.Token);
            selectedResource = snapshot.Resources.FirstOrDefault(); page = 0; filterMode = "All";
            ShowResults();
        }
        finally { busy = false; progressBar.Visibility = Visibility.Collapsed; if (analyzeButton != null) analyzeButton.IsEnabled = true; if (cancelButton != null) cancelButton.Visibility = Visibility.Collapsed; }
    }
    private void ShowResults()
    {
        SelectNavigation("Analyze"); currentPage = "Results";
        if (snapshot == null) { var empty = Page(T("Empty"), T("EmptyDetail")); empty.Children.Add(Button(T("AnalyzeFolder"), ShowAnalyze, true)); return; }
        DetachInputs(); body.Children.Clear();
        var layout = resultLayout = new Grid { Margin = new(28, 24, 28, 16), RowSpacing = 16 };
        layout.RowDefinitions.Add(new() { Height = GridLength.Auto }); layout.RowDefinitions.Add(new() { Height = GridLength.Auto }); layout.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
        var header = new StackPanel { Spacing = 8 };
        header.Children.Add(Text(T("Results"), 28, true));
        if (IsDemo) header.Children.Add(DemoBanner());
        header.Children.Add(Mono(snapshot.Root));
        header.Children.Add(Text($"{T(snapshot.Cancelled ? "Cancelled" : "Complete")} · {snapshot.CreatedAt.ToLocalTime():g} · {snapshot.CompletedAt - snapshot.CreatedAt:mm\\:ss}", 12));
        var more = new MenuFlyout();
        void Menu(string key, Func<Task> action) { var item = new MenuFlyoutItem { Text = T(key) }; item.Click += (_, _) => Run(action); more.Items.Add(item); }
        Menu("Import", ImportAsync); Menu("Schedule", ScheduleAsync);
        var moreButton = Button(T("More"), () => { }); moreButton.Flyout = more;
        header.Children.Add(Row(Button(T(saved ? "Saved" : "Save"), () => Run(SaveAsync)), Button(T("Export"), () => Run(ExportAsync)), Button(T("Analyze"), () => Run(AnalyzeAsync)), moreButton));
        layout.Children.Add(header);
        var stats = new Grid { ColumnSpacing = 12 };
        for (var i = 0; i < 3; i++) stats.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
        (string Key, int Count, string Filter)[] numbers = [("Objects", snapshot.Resources.Count, "All"), ("Findings", snapshot.Resources.Sum(r => r.Findings.Count), "FilterFindings"), ("Errors", snapshot.Resources.Count(r => r.Error != null), "FilterUnknown")];
        for (var i = 0; i < numbers.Length; i++)
        {
            var number = numbers[i]; var content = Text(number.Count.ToString("N0") + " · " + T(number.Key), 14, true);
            var stat = Button(T(number.Key), () => { filterMode = number.Filter; page = 0; RefreshObjects(); }); stat.Content = content; stat.HorizontalAlignment = HorizontalAlignment.Stretch; stat.HorizontalContentAlignment = HorizontalAlignment.Left;
            stat.Background = new SolidColorBrush(Colors.Transparent); stat.BorderThickness = new(0, 0, 0, 1); stat.BorderBrush = new SolidColorBrush(global::Windows.UI.Color.FromArgb(48, 100, 107, 118)); stat.CornerRadius = new(0); stat.Padding = new(0, 10, 0, 14);
            stats.Children.Add(stat); Grid.SetColumn(stat, i);
        }
        layout.Children.Add(stats); Grid.SetRow(stats, 1);
        var split = resultSplit = new Grid { ColumnSpacing = 22, RowSpacing = 12 };
        split.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) }); split.RowDefinitions.Add(new() { Height = new(0) });
        split.ColumnDefinitions.Add(new() { Width = new(0.38, GridUnitType.Star), MinWidth = 240 }); split.ColumnDefinitions.Add(new() { Width = new(0.62, GridUnitType.Star), MinWidth = 320 });
        var left = new Grid { RowSpacing = 8 };
        left.RowDefinitions.Add(new() { Height = GridLength.Auto }); left.RowDefinitions.Add(new() { Height = GridLength.Auto }); left.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) }); left.RowDefinitions.Add(new() { Height = GridLength.Auto });
        AddInput(left, filterBox);
        var mode = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
        foreach (var key in new[] { "All", "FilterModify", "FilterUnknown", "FilterFindings" }) mode.Items.Add(new ComboBoxItem { Content = T(key), Tag = key });
        mode.SelectedIndex = Array.IndexOf(new[] { "All", "FilterModify", "FilterUnknown", "FilterFindings" }, filterMode);
        mode.SelectionChanged += (_, _) => { filterMode = ((ComboBoxItem)mode.SelectedItem).Tag.ToString()!; page = 0; RefreshObjects(); };
        left.Children.Add(mode); Grid.SetRow(mode, 1);
        AddInput(left, objectList); Grid.SetRow(objectList, 2);
        pageLabel = Text("", 11); var pages = Row(Button("‹", () => { page = Math.Max(0, page - 1); RefreshObjects(); }), pageLabel, Button("›", () => { page++; RefreshObjects(); }));
        left.Children.Add(pages); Grid.SetRow(pages, 3);
        split.Children.Add(left); resultList = left;
        if (detailHost != null) detailHost.Content = null;
        var right = detailHost = new ScrollViewer { Content = detail, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        AutomationProperties.SetAutomationId(right, "AccessDetail");
        split.Children.Add(right); Grid.SetColumn(right, 1); resultDetail = right;
        layout.Children.Add(split); Grid.SetRow(split, 2);
        body.Children.Add(new ScrollViewer { Content = layout, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        RefreshObjects(); RenderDetail(); AdaptLayout();
    }
    private void RefreshObjects()
    {
        if (snapshot == null) return;
        var query = filterBox.Text.Trim();
        var resources = snapshot.Resources.Where(r =>
            (query.Length == 0 || r.Path.Contains(query, StringComparison.CurrentCultureIgnoreCase) || r.Decision?.Identity.Contains(query, StringComparison.CurrentCultureIgnoreCase) == true || r.Descriptor?.Aces.Any(a => a.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) || a.Sid.Contains(query, StringComparison.OrdinalIgnoreCase)) == true) &&
            (filterMode switch { "FilterFindings" => r.Findings.Count > 0, "FilterUnknown" => r.Error != null || r.Decision?.State == AccessState.Unknown, "FilterModify" => r.Decision is { State: not AccessState.Unknown } d && (d.EffectiveMask & 0x1301BF) == 0x1301BF, _ => true })).ToArray();
        const int pageSize = 150;
        var pages = Math.Max(1, (resources.Length + pageSize - 1) / pageSize); page = Math.Clamp(page, 0, pages - 1);
        objectList.Items.Clear();
        foreach (var r in resources.Skip(page * pageSize).Take(pageSize))
        {
            var content = new StackPanel { Spacing = 5, Margin = new(0, 6, 0, 6) };
            var name = System.IO.Path.GetFileName(r.Path.TrimEnd('\\')); content.Children.Add(Text(name.Length == 0 ? r.Path : name, 14, true));
            content.Children.Add(Text(T(r.Decision?.State.ToString() ?? "Unknown") + (r.Findings.Count > 0 ? $" · {r.Findings.Count} {T("Findings")}" : ""), 11));
            var item = new ListViewItem { Content = content, Tag = r, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            ToolTipService.SetToolTip(item, r.Path); AutomationProperties.SetName(item, r.Path + " " + T(r.Decision?.State.ToString() ?? "Unknown")); objectList.Items.Add(item);
            if (r.Path == selectedResource?.Path) objectList.SelectedItem = item;
        }
        if (pageLabel != null) pageLabel.Text = $"{page + 1} / {pages} · {resources.Length:N0}";
    }
    private void RenderDetail()
    {
        detail.Children.Clear();
        if (selectedResource is not { } resource) { detail.Children.Add(Text(T("NoSelection"))); return; }
        detail.Children.Add(Text(System.IO.Path.GetFileName(resource.Path.TrimEnd('\\')) is { Length: > 0 } name ? name : resource.Path, 24, true));
        detail.Children.Add(Mono(resource.Path));
        detail.Children.Add(Text(T(resource.Share == null ? "LocalScope" : "RemoteScope"), 12));
        var tabs = new FlowPanel { Spacing = 6 };
        var keys = new[] { "Access", "Why", "Findings", "Permissions", "Technical", "Simulate" };
        foreach (var key in keys)
        {
            var tab = new ToggleButton { Content = T(key), IsChecked = key == activeTab, FontSize = 12, MinHeight = 34, Padding = new(10, 6, 10, 6) };
            tab.Background = new SolidColorBrush(Colors.Transparent); tab.BorderThickness = new(0, 0, 0, 2); tab.CornerRadius = new(0);
            tab.BorderBrush = key == activeTab ? Brush(39, 100, 231) : new SolidColorBrush(Colors.Transparent);
            tab.Foreground = HighContrast ? SystemBrush(true) : root.ActualTheme == ElementTheme.Dark ? Brush(242, 243, 245) : Brush(23, 25, 29);
            if (HighContrast) tab.BorderBrush = SystemBrush(true);
            AutomationProperties.SetAutomationId(tab, "Tab" + key);
            tab.Click += (_, _) => { activeTab = key; RenderDetail(); };
            tabs.Children.Add(tab);
        }
        detail.Children.Add(tabs);
        if (resource.Error != null) detail.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning, Title = T("Errors"), Message = $"{resource.Error.Code}: {resource.Error.Message}" });
        var d = resource.Decision;
        if (d is null || d.State == AccessState.Unknown)
        {
            var unknown = new StackPanel { Spacing = 8 };
            foreach (var pair in new[] { ("WhatWeKnow", resource.Descriptor != null ? "KnownDescriptor" : "UnknownRead"), ("CouldNotVerify", AccessSummary.UnknownReason(resource)), ("WhyItMatters", "UnknownMeaning"), ("NextStep", "UnknownNext") })
            { unknown.Children.Add(Text(T(pair.Item1), 14, true)); unknown.Children.Add(Text(T(pair.Item2), 13)); }
            detail.Children.Add(new Expander { Header = T("SummaryUnknown"), Content = unknown, IsExpanded = true, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch });
        }
        switch (activeTab)
        {
            case "Access":
                detail.Children.Add(Text(T("EvaluatedIdentity"), 12)); detail.Children.Add(Text(d?.Identity ?? T("Unknown"), 18, true));
                detail.Children.Add(Text(T(AccessSummary.Key(d)), 22, true));
                detail.Children.Add(Text(T("ScopeCaution"), 12));
                detail.Children.Add(Text(T("AccessExplanation"), 12));
                detail.Children.Add(new Expander { Header = T("TechnicalDetails"), Content = Text(T("TechnicalScope"), 12), HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch });
                if (d == null) break;
                foreach (var c in d.Capabilities)
                {
                    var row = new Grid(); row.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) }); row.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
                    row.Children.Add(Text(T(c.Key))); var state = Text(T(c.State.ToString()), 13, true); row.Children.Add(state); Grid.SetColumn(state, 1); detail.Children.Add(row);
                }
                if (resource.Share != null) { detail.Children.Add(Line()); detail.Children.Add(Mono($"{T("Ntfs")}: 0x{d.GrantedMask:X8}\n{T("Share")}: {(d.ShareMask is { } mask ? $"0x{mask:X8}" : T("Unknown"))}")); }
                detail.Children.Add(Button(T("Why") + " →", () => { activeTab = "Why"; RenderDetail(); }));
                break;
            case "Why": RenderEvidence(resource); break;
            case "Permissions":
                detail.Children.Add(Text(T("Principals"), 18, true)); detail.Children.Add(Text(T("PrincipalsNote"), 12));
                foreach (var ace in resource.Descriptor?.Aces ?? [])
                {
                    var content = new StackPanel { Spacing = 8 }; content.Children.Add(Text(ace.Name, 14, true)); content.Children.Add(Mono(ace.Sid));
                    content.Children.Add(Mono($"ACE #{ace.Index} · {ace.Type} · 0x{ace.Mask:X8}\n{ace.Flags}"));
                    content.Children.Add(Text(T(ace.Inherited ? "Inherited" : "Explicit"), 12));
                    if (ace.Inherited) content.Children.Add(Text(T("InheritedSourceNote"), 12));
                    detail.Children.Add(new Expander { Header = $"#{ace.Index}  {ace.Name}", Content = content, HorizontalAlignment = HorizontalAlignment.Stretch });
                }
                break;
            case "Findings":
                if (resource.Findings.Count == 0) detail.Children.Add(Text(T("NoFindings")));
                foreach (var f in resource.Findings)
                {
                    detail.Children.Add(Text(T(f.Severity.ToString()), 13, true)); detail.Children.Add(Text(f.Evidence)); detail.Children.Add(Text(f.Recommendation, 12)); detail.Children.Add(Line());
                }
                break;
            case "Technical":
                detail.Children.Add(Text(T("TechnicalScope"), 12));
                if (d != null) detail.Children.Add(Text(d.Basis, 12));
                if (d?.Limitation != null) detail.Children.Add(Text(d.Limitation, 12));
                detail.Children.Add(Button(T("CopyDiagnostic"), () =>
                {
                    var data = new DataPackage(); data.SetText(AccessSummary.Diagnostic("Inspect", resource.Error?.Code, d?.State, resource.Share != null)); Clipboard.SetContent(data); Notify(T("Copied"));
                }));
                if (resource.Descriptor is { } sd)
                {
                    detail.Children.Add(Mono($"{T("Owner")}: {sd.Owner}\n{T("Protected")}: {sd.Protected}\n{T("NullDacl")}: {sd.NullDacl}\n{T("Canonical")}: {sd.Canonical}\nSHA-256: {sd.Hash}"));
                    detail.Children.Add(Mono(sd.Sddl));
                    detail.Children.Add(Button(T("Copy"), () => { var data = new DataPackage(); data.SetText(JsonSerializer.Serialize(resource, SnapshotJson.Options)); Clipboard.SetContent(data); Notify(T("Copied")); }));
                }
                if (resource.Share is { } share) detail.Children.Add(Mono(JsonSerializer.Serialize(share, SnapshotJson.Options)));
                if (snapshot?.DirectoryIdentity is { } directoryIdentity) detail.Children.Add(Mono(JsonSerializer.Serialize(directoryIdentity, SnapshotJson.Options)));
                foreach (var warning in snapshot?.IdentityWarnings ?? []) detail.Children.Add(Text(warning, 12));
                break;
            case "Simulate": RenderSimulation(resource); break;
        }
    }
    private void RenderEvidence(ResourceAccess resource)
    {
        var decision = resource.Decision;
        if (decision == null) return;
        detail.Children.Add(Text(decision.Identity, 18, true));
        detail.Children.Add(Text(T("Contributors"), 12));
        detail.Children.Add(Text(T("EvidenceIntro"), 13));
        if (decision.Evidence.Count == 0) detail.Children.Add(Text(T("NoEvidence")));
        foreach (var evidence in decision.Evidence)
        {
            var content = new StackPanel { Spacing = 10 };
            if (evidence.MembershipPath.Count > 0)
                foreach (var edge in evidence.MembershipPath) content.Children.Add(Mono($"{edge.MemberSid}\n↓\n{edge.GroupSid}\n{edge.Source}"));
            else if (evidence.Sid != decision.Sid && evidence.AceIndex >= 0)
            { content.Children.Add(Text(T("TokenMembership"), 13, true)); content.Children.Add(Text(T("MembershipCaution"), 12)); }
            content.Children.Add(Mono(evidence.Sid));
            content.Children.Add(Text(T(evidence.Relation), 14, true));
            content.Children.Add(Mono($"ACE #{evidence.AceIndex}\n{T("RawMask")}: 0x{evidence.Mask:X8}\n{T("Contribution")}: 0x{evidence.ContributingMask:X8}\n{evidence.Flags}\n{evidence.Source}"));
            if (evidence.Flags.Contains("Inherited")) content.Children.Add(Text(T("InheritedSourceNote"), 12));
            var header = new StackPanel { Spacing = 4 }; header.Children.Add(Text(evidence.Identity, 14, true)); header.Children.Add(Text($"{T(evidence.Relation)} · {MaskSummary(evidence.ContributingMask)}", 12));
            var branch = new Grid { ColumnSpacing = 14 }; branch.ColumnDefinitions.Add(new() { Width = new(18) }); branch.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
            var rail = new Border { Width = 2, HorizontalAlignment = HorizontalAlignment.Center, Background = Brush(100, 107, 118), Opacity = 0.4, Margin = new(0, 12, 0, 0) }; branch.Children.Add(rail);
            var node = new Ellipse { Width = 10, Height = 10, VerticalAlignment = VerticalAlignment.Top, Margin = new(0, 20, 0, 0), Fill = evidence.Relation == "DeniedBy" ? Brush(200, 58, 58) : Brush(39, 100, 231) }; branch.Children.Add(node);
            var proof = new Expander { Header = header, Content = content, IsExpanded = false, HorizontalAlignment = HorizontalAlignment.Stretch, BorderThickness = new(0) };
            AutomationProperties.SetName(proof, evidence.Identity + " " + T(evidence.Relation) + " " + MaskSummary(evidence.ContributingMask));
            branch.Children.Add(proof); Grid.SetColumn(proof, 1); detail.Children.Add(branch);
        }
        detail.Children.Add(Text("↓", 20)); detail.Children.Add(Text(resource.Path, 15, true));
        detail.Children.Add(Text(T(decision.State.ToString()), 20, true));
    }
    private static string MaskSummary(uint mask)
    {
        if (mask == 0) return T("NoAdditionalBits");
        if ((mask & Rights.FullControl) == Rights.FullControl) return T("FullControl");
        if ((mask & 0x1301BF) == 0x1301BF) return T("Modify");
        var names = new List<string>();
        foreach (var capability in Rights.Capabilities.Where(c => c.Key is "Read" or "Write" or "Delete" or "ChangePermissions" or "TakeOwnership"))
            if ((mask & capability.Mask) == capability.Mask) names.Add(T(capability.Key));
        return names.Count == 0 ? $"0x{mask:X8}" : string.Join(" · ", names);
    }
    private void RenderSimulation(ResourceAccess resource)
    {
        detail.Children.Add(Text(T("SimulationOnly"), 11, true)); detail.Children.Add(Text(T("SimulationTitle"), 20, true)); detail.Children.Add(Text(T("SimulationNote"), 13));
        if (resource.Descriptor is not { Aces.Count: > 0 } descriptor) return;
        var ace = new ComboBox { Header = T("AceIndex"), HorizontalAlignment = HorizontalAlignment.Stretch };
        foreach (var entry in descriptor.Aces) ace.Items.Add(new ComboBoxItem { Content = $"#{entry.Index} · {entry.Name} · {entry.Type}", Tag = entry.Index });
        ace.SelectedIndex = 0; detail.Children.Add(ace);
        var output = new StackPanel { Spacing = 10 };
        var calculate = Button(T("Recalculate"), () => Run(async () =>
        {
            var index = (int)((ComboBoxItem)ace.SelectedItem).Tag;
            var selectedSid = snapshot?.IdentitySid;
            var result = await Task.Run(() =>
            {
                if (IsDemo) return (Before: DemoFixture.Evaluate(descriptor.Sddl, resource.Path, resource.Share != null), After: DemoFixture.Evaluate(ImpactSimulator.RemoveAce(descriptor.Sddl, index), resource.Path, resource.Share != null));
                var resolver = new IdentityResolver(); using var access = new EffectiveAccessResolver(resolver, selectedSid);
                var proposed = DescriptorParser.Parse(ImpactSimulator.RemoveAce(descriptor.Sddl, index));
                return (Before: access.Evaluate(descriptor, resource.Path, resource.Share, resource.IsReparsePoint), After: access.Evaluate(proposed, resource.Path, resource.Share, resource.IsReparsePoint));
            });
            output.Children.Clear();
            AutomationProperties.SetAutomationId(output, "SimulationResult");
            output.Children.Add(Text(T(AccessSummary.Key(result.After)), 18, true));
            output.Children.Add(Mono($"{T("Before")}: 0x{result.Before.EffectiveMask:X8}\n{T("After")}: 0x{result.After.EffectiveMask:X8}\n{T("Gained")}: 0x{(result.After.EffectiveMask & ~result.Before.EffectiveMask):X8}\n{T("Lost")}: 0x{(result.Before.EffectiveMask & ~result.After.EffectiveMask):X8}"));
            if (result.After.Limitation != null) output.Children.Add(Text(result.After.Limitation));
            foreach (var capability in result.After.Capabilities) output.Children.Add(Text($"{T(capability.Key)} · {T(capability.State.ToString())}", 13));
            if (!IsDemo && !resource.IsDirectory && !resource.IsReparsePoint && resource.Share == null && !descriptor.HasSpecialAces)
                output.Children.Add(Button(T("ApplyChange"), () => Run(() => ApplyChangeAsync(resource, ImpactSimulator.RemoveAce(descriptor.Sddl, index), output))));
        }), true);
        AutomationProperties.SetAutomationId(calculate, "RunSimulation");
        detail.Children.Add(calculate); detail.Children.Add(output);
    }
    private async Task ApplyChangeAsync(ResourceAccess resource, string proposed, StackPanel output)
    {
        var plan = await Task.Run(() => RemediationService.Prepare(resource.Path, resource.Descriptor!.Hash, proposed));
        var confirmation = new TextBox { Header = T("ConfirmPath"), FlowDirection = FlowDirection.LeftToRight };
        var content = new StackPanel { Spacing = 12 }; content.Children.Add(Text(T("ChangeWarning"))); content.Children.Add(Mono(resource.Path)); content.Children.Add(confirmation);
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = T("ApplyChange"), Content = content, PrimaryButtonText = T("Apply"), CloseButtonText = T("Cancel"), IsPrimaryButtonEnabled = false };
        confirmation.TextChanged += (_, _) => dialog.IsPrimaryButtonEnabled = string.Equals(confirmation.Text, resource.Path, StringComparison.OrdinalIgnoreCase);
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        var current = snapshot;
        if (!saved && current != null) { await Task.Run(() => new SnapshotStore().Save(current)); saved = true; }
        var receipt = await Task.Run(() => RemediationService.Apply(plan, resource.Path));
        output.Children.Add(Text(T("ChangeVerified"), 15, true));
        var rollback = Button(T("Rollback"), () => Run(async () =>
        {
            var confirm = new ContentDialog { XamlRoot = root.XamlRoot, Title = T("Rollback"), Content = resource.Path, PrimaryButtonText = T("Apply"), CloseButtonText = T("Cancel") };
            if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;
            receipt = await Task.Run(() => RemediationService.Rollback(receipt, resource.Path)); Notify(T("RollbackVerified"));
        })); output.Children.Add(rollback);
    }
    private async Task SaveAsync()
    {
        if (snapshot == null || saved || savePending) return;
        savePending = true;
        try
        {
            var current = snapshot; await Task.Run(() => new SnapshotStore().Save(current));
            if (ReferenceEquals(snapshot, current)) { saved = true; ShowResults(); }
            Notify(T("Saved"));
        }
        finally { savePending = false; }
    }
    private async Task ShowSnapshotsAsync()
    {
        SelectNavigation("Scans");
        var list = await Task.Run(() => new SnapshotStore().List());
        var panel = Page(T("Scans"), T("SnapshotIntro"));
        panel.Children.Add(Button(T("Import"), () => Run(ImportAsync)));
        if (list.Count == 0) panel.Children.Add(Text(T("NoSnapshots")));
        foreach (var item in list)
        {
            var content = new StackPanel { Spacing = 6 }; content.Children.Add(Text(item.Root, 16, true)); content.Children.Add(Text($"{item.CreatedAt.ToLocalTime():g} · {item.Objects:N0} {T("Objects")}", 12));
            var button = Button(T("Open"), () => Run(async () => { snapshot = await Task.Run(() => new SnapshotStore().Load(item.Id)); saved = true; selectedResource = snapshot.Resources.FirstOrDefault(); pathBox.Text = snapshot.Root; ShowResults(); }));
            button.Content = content; button.HorizontalAlignment = HorizontalAlignment.Stretch; button.HorizontalContentAlignment = HorizontalAlignment.Left; panel.Children.Add(button);
        }
    }
    private async Task ImportAsync()
    {
        var picker = new FileOpenPicker(AppWindow.Id); picker.FileTypeFilter.Add(".json");
        var file = await picker.PickSingleFileAsync(); if (file == null) return;
        var imported = await Task.Run(() => SnapshotJson.Parse(File.ReadAllText(file.Path)));
        snapshot = imported; saved = false; selectedResource = imported.Resources.FirstOrDefault(); pathBox.Text = imported.Root; ShowResults();
    }
    private async Task CompareAsync()
    {
        SelectNavigation("Compare");
        var demo = IsDemo;
        var demoSnapshots = demo ? new[] { DemoFixture.Create(), DemoFixture.Create(true) } : Array.Empty<PermissionSnapshot>();
        IReadOnlyList<SnapshotSummary> list = demo ? demoSnapshots.Reverse().Select(s => new SnapshotSummary(s.Id, s.Root, s.CreatedAt, s.Resources.Count, s.Cancelled)).ToArray() : await Task.Run(() => new SnapshotStore().List());
        var panel = Page(T("Compare"), T("CompareIntro"));
        if (list.Count < 2) { panel.Children.Add(Text(T("NoSnapshots"))); return; }
        ComboBox Picker(string key) { var box = new ComboBox { Header = T(key), HorizontalAlignment = HorizontalAlignment.Stretch }; foreach (var s in list) box.Items.Add(new ComboBoxItem { Content = $"{s.CreatedAt.ToLocalTime():g} · {s.Root}", Tag = s.Id }); return box; }
        var before = Picker("Baseline"); var after = Picker("Later"); before.SelectedIndex = 1; after.SelectedIndex = 0; panel.Children.Add(before); panel.Children.Add(after);
        var output = new StackPanel { Spacing = 16 };
        var compare = Button(T("Compare"), () => Run(async () =>
        {
            var a = (string)((ComboBoxItem)before.SelectedItem).Tag; var b = (string)((ComboBoxItem)after.SelectedItem).Tag;
            var changes = await Task.Run(() => { if (demo) return SnapshotComparer.Compare(demoSnapshots.Single(s => s.Id == a), demoSnapshots.Single(s => s.Id == b)); var store = new SnapshotStore(); return SnapshotComparer.Compare(store.Load(a), store.Load(b)); });
            output.Children.Clear(); if (changes.Count == 0) output.Children.Add(Text(T("NoChanges")));
            AutomationProperties.SetAutomationId(output, "ComparisonResult");
            foreach (var c in changes.Take(500)) { output.Children.Add(Text(T(c.Kind) + " · " + c.Path, 14, true)); output.Children.Add(Text(c.Detail, 13)); }
            if (changes.Count > 500) output.Children.Add(Text($"500 / {changes.Count:N0}"));
        }), true);
        AutomationProperties.SetAutomationId(compare, "RunComparison");
        panel.Children.Add(compare); panel.Children.Add(output);
    }
    private async Task ExportAsync()
    {
        if (snapshot == null) return;
        var picker = new FileSavePicker(AppWindow.Id) { SuggestedFileName = "PermissionScope-" + snapshot.CreatedAt.ToString("yyyyMMdd-HHmm") };
        picker.FileTypeChoices.Add("HTML", [".html"]); picker.FileTypeChoices.Add("PDF", [".pdf"]); picker.FileTypeChoices.Add("Excel", [".xlsx"]); picker.FileTypeChoices.Add("CSV", [".csv"]); picker.FileTypeChoices.Add("JSON", [".json"]);
        var file = await picker.PickSaveFileAsync(); if (file == null) return;
        var current = snapshot; await Task.Run(() => ReportExporter.Export(current, file.Path, System.IO.Path.GetExtension(file.Path).TrimStart('.'))); Notify(T("Exported") + " · " + file.Path);
    }
    private async Task ScheduleAsync()
    {
        if (snapshot == null) return;
        if (IsDemo) { Notify(T("DemoNote")); return; }
        var time = new TextBox { Header = T("ScheduleTime"), Text = "09:00" };
        var content = new StackPanel { Spacing = 16 }; content.Children.Add(Text(T("ScheduleIntro"))); content.Children.Add(Mono(snapshot.Root)); content.Children.Add(time);
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = T("Schedule"), Content = content, PrimaryButtonText = T("Apply"), CloseButtonText = T("Cancel") };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        var rootPath = snapshot.Root;
        await Task.Run(() => ScanScheduler.CreateDaily(rootPath, time.Text)); Notify(T("ScheduleCreated"));
    }
    private void ShowSettings()
    {
        SelectNavigation("Settings");
        var panel = Page(T("Settings"));
        var appearance = new ComboBox { Header = T("Appearance"), MinWidth = 220 };
        AutomationProperties.SetAutomationId(appearance, "AppearanceChoice");
        foreach (var key in new[] { "System", "Light", "Dark" }) appearance.Items.Add(new ComboBoxItem { Content = T(key), Tag = key });
        appearance.SelectedIndex = Math.Max(0, Array.IndexOf(new[] { "System", "Light", "Dark" }, Localization.Preferences.Appearance));
        var language = new ComboBox { Header = T("Language"), MinWidth = 220 };
        AutomationProperties.SetAutomationId(language, "LanguageChoice");
        var languages = new[] { "system" }.Concat(Available);
#if DEBUG
        languages = languages.Concat(["qps-ploc", "qps-rtl"]);
#endif
        foreach (var key in languages)
        {
            var name = key == "system" ? T("System") : key.StartsWith("qps-") ? key : CultureInfo.GetCultureInfo(key).NativeName;
            if (key is not ("system" or "en-US" or "pt-BR") && !key.StartsWith("qps-")) name += " · " + T("TranslationPreview");
            var item = new ComboBoxItem { Content = name, Tag = key };
            AutomationProperties.SetAutomationId(item, "Language_" + key);
            language.Items.Add(item); if (key == Localization.Preferences.Language) language.SelectedItem = item;
        }
        if (language.SelectedItem == null) language.SelectedIndex = 0;
        panel.Children.Add(appearance); panel.Children.Add(language);
        panel.Children.Add(Text(T("TranslationNote"), 12));
        var applyPreferences = Button(T("Apply"), () => Run(() =>
        {
            Localization.Save((language.SelectedItem as ComboBoxItem)?.Tag as string ?? "system", (appearance.SelectedItem as ComboBoxItem)?.Tag as string ?? "System");
            ApplyAppearance(); BuildNavigation(); pathBox.Header = T("Path"); identityBox.Header = T("Identity"); identityBox.PlaceholderText = T("IdentityHint"); filterBox.PlaceholderText = T("Filter"); ShowSettings(); return Task.CompletedTask;
        }), true);
        AutomationProperties.SetAutomationId(applyPreferences, "ApplyPreferences");
        panel.Children.Add(applyPreferences);
        panel.Children.Add(Line()); panel.Children.Add(Text(T("About"), 22, true)); panel.Children.Add(Text("PermissionScope 1.0.0", 17, true)); panel.Children.Add(Text(T("Author"), 13)); panel.Children.Add(Text(T("AboutText"))); panel.Children.Add(Text(T("FreeForever"), 14, true));
        panel.Children.Add(Text(T("SimpleExplanation")));
        panel.Children.Add(new Expander { Header = T("TechnicalDetails"), Content = Text(T("TechnicalScope"), 12), HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch });
        panel.Children.Add(Button(T("DataFolder"), () => Run(() => { Directory.CreateDirectory(SnapshotStore.DefaultDirectory); Process.Start(new ProcessStartInfo(SnapshotStore.DefaultDirectory) { UseShellExecute = true }); return Task.CompletedTask; })));
    }
    private async void Run(Func<Task> action)
    {
        try { notice.IsOpen = false; notice.ActionButton = null; await action(); }
        catch (OperationCanceledException) { Notify(T("Cancelled")); }
        catch (Exception error)
        {
            var diagnostic = $"{DateTimeOffset.Now:O}\nPage: {currentPage}\nOperation: {action.Method.Name}\n{error}";
            try { AtomicFile.WriteText(System.IO.Path.Combine(SnapshotStore.DefaultDirectory, "last-operation-error.log"), diagnostic); }
            catch (Exception loggingError) when (loggingError is IOException or UnauthorizedAccessException) { Debug.WriteLine(loggingError); }
            notice.Title = T("ErrorTitle");
            notice.Message = error switch
            {
                UnauthorizedAccessException => T("ErrorAccess"),
                IOException => T("ErrorFile"),
                COMException => T("ErrorWindows"),
                _ => error.Message
            };
            notice.ActionButton = Button(T("TechnicalDetails"), async () =>
            {
                var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = T("TechnicalDetails"), CloseButtonText = T("Close"), Content = new ScrollViewer { MaxHeight = 360, Content = Mono(diagnostic) } };
                try { await dialog.ShowAsync(); } catch (COMException) { Debug.WriteLine(diagnostic); }
            });
            notice.Severity = InfoBarSeverity.Error; notice.IsOpen = true;
        }
    }
    private void Notify(string message) { notice.Title = "PermissionScope"; notice.Message = message; notice.Severity = InfoBarSeverity.Informational; notice.IsOpen = true; }
}
