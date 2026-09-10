using Microsoft.UI.Xaml;

namespace PermissionScope.App;

public partial class App : Application
{
    private MainWindow? window;
    public App()
    {
        UnhandledException += (_, e) => WriteFailure(e.Exception);
        InitializeComponent();
    }
    private static void WriteFailure(Exception error)
    {
        try { Core.AtomicFile.WriteText(System.IO.Path.Combine(Core.SnapshotStore.DefaultDirectory, "last-error.log"), $"{DateTimeOffset.UtcNow:O}\n{error}"); }
        catch (Exception loggingError) when (loggingError is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(loggingError.Message); }
    }
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            window = new MainWindow();
            window.Activate();
            var path = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault(a => !a.StartsWith("--"));
            if (path != null) window.SetPath(path);
        }
        catch (Exception error) { WriteFailure(error); throw; }
    }
}
