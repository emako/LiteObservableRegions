using System;

namespace LiteObservableRegions;

/// <summary>
/// Event arguments for region content change (before the change is applied).
/// Set <see cref="Cancel"/> to true to prevent the navigation.
/// </summary>
public sealed class RegionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Region name (e.g. "MainRegion").
    /// </summary>
    public string RegionName { get; }

    /// <summary>
    /// URI of the current view before this change (null if none).
    /// </summary>
    public Uri FromUri { get; }

    /// <summary>
    /// Target URI of this navigation.
    /// </summary>
    public Uri ToUri { get; }

    /// <summary>
    /// Target name of the view being left (from FromUri path segment).
    /// </summary>
    public string FromTargetName { get; }

    /// <summary>
    /// Target name of the view being navigated to (from ToUri path segment).
    /// </summary>
    public string ToTargetName { get; }

    /// <summary>
    /// How the navigation was initiated: Navigate (Push), Redirect (Replace), GoBack, or GoForward.
    /// </summary>
    public NavigationMode Mode { get; }

    /// <summary>
    /// Set to true to cancel the navigation. No stack or content change will occur.
    /// </summary>
    public bool Cancel { get; set; }

    public RegionChangedEventArgs(
        string regionName,
        Uri fromUri,
        Uri toUri,
        string fromTargetName,
        string toTargetName,
        NavigationMode mode)
    {
        RegionName = regionName ?? throw new ArgumentNullException(nameof(regionName));
        FromUri = fromUri;
        ToUri = toUri ?? throw new ArgumentNullException(nameof(toUri));
        FromTargetName = fromTargetName ?? string.Empty;
        ToTargetName = toTargetName ?? throw new ArgumentNullException(nameof(toTargetName));
        Mode = mode;
    }
}
