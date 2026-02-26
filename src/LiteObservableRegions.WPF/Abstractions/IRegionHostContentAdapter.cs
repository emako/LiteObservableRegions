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
    public void SetContent(DependencyObject host, object content);
}
