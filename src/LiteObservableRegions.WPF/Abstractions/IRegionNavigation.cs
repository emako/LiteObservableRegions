using System;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Navigation operations for region-based navigation (history and redirect).
/// </summary>
public interface IRegionNavigation
{
    /// <summary>
    /// Navigate to the given region URI (push onto back stack).
    /// </summary>
    /// <param name="uri">Full region URI, e.g. region://MainRegion/ViewA.</param>
    public void Navigate(Uri uri);

    /// <summary>
    /// Redirect to the given region URI (replace current, clear back stack).
    /// </summary>
    /// <param name="uri">Full region URI.</param>
    public void Redirect(Uri uri);

    /// <summary>
    /// Go back in the specified region.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    public void GoBack(string regionName);

    /// <summary>
    /// Go forward in the specified region.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    public void GoForward(string regionName);

    /// <summary>
    /// Whether the region can go back.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>True if the region has at least one entry on the back stack.</returns>
    public bool CanGoBack(string regionName);

    /// <summary>
    /// Whether the region can go forward.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>True if the region has at least one entry on the forward stack.</returns>
    public bool CanGoForward(string regionName);
}
