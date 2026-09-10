using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace PermissionScope.App;

internal sealed class FlowPanel : Panel
{
    public double Spacing { get; set; } = 10;
    protected override Size MeasureOverride(Size available)
    {
        double x = 0, y = 0, rowHeight = 0, width = 0;
        foreach (var child in Children)
        {
            child.Measure(new Size(available.Width, available.Height));
            var size = child.DesiredSize;
            if (x > 0 && x + size.Width > available.Width) { y += rowHeight + Spacing; x = 0; rowHeight = 0; }
            width = Math.Max(width, x + size.Width); x += size.Width + Spacing; rowHeight = Math.Max(rowHeight, size.Height);
        }
        return new Size(width, y + rowHeight);
    }
    protected override Size ArrangeOverride(Size available)
    {
        double x = 0, y = 0, rowHeight = 0;
        foreach (var child in Children)
        {
            var size = child.DesiredSize;
            if (x > 0 && x + size.Width > available.Width) { y += rowHeight + Spacing; x = 0; rowHeight = 0; }
            child.Arrange(new Rect(x, y, Math.Min(size.Width, available.Width), size.Height));
            x += size.Width + Spacing; rowHeight = Math.Max(rowHeight, size.Height);
        }
        return available;
    }
}
