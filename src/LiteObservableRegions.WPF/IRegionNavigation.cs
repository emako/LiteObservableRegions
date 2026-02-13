using System;

namespace LiteObservableRegions;

/// <summary>
/// Navigation operations for region-based navigation (history and redirect).
/// </summary>
public interface IRegionNavigation
{
    /// <summary>
    /// Navigate to the given region URI (push onto back stack).
    /// </summary>
    public void Navigate(Uri uri);

    /// <summary>
    /// Redirect to the given region URI (replace current, no back stack).
    /// </summary>
    public void Redirect(Uri uri);

    /// <summary>
    /// Go back in the specified region.
    /// </summary>
    public void GoBack(string regionName);

    /// <summary>
    /// Go forward in the specified region.
    /// </summary>
    public void GoForward(string regionName);

    /// <summary>
    /// Whether the region can go back.
    /// </summary>
    public bool CanGoBack(string regionName);

    /// <summary>
    /// Whether the region can go forward.
    /// </summary>
    public bool CanGoForward(string regionName);
}
