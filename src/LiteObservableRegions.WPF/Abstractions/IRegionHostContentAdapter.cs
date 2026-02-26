using System.Windows;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Strategy for how to display the resolved view in a region host.
/// Register your own implementation in DI to customize (e.g. support Grid, custom panels).
/// </summary>
public interface IRegionHostContentAdapter
{
    /// <summary>
    /// Displays the given content in the host (e.g. set ContentControl.Content or attach to host).
    /// </summary>
    /// <param name="host">The region host element (DependencyObject with RegionName).</param>
    /// <param name="content">The view to display (typically a UIElement).</param>
    public void SetContent(DependencyObject host, object content);
}
