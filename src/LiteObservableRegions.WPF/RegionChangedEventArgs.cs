using System;
using System.Diagnostics;

namespace LiteObservableRegions;

/// <summary>
/// Event arguments for region content change (before the change is applied).
/// Set <see cref="Cancel"/> to true to prevent the navigation.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class RegionChangedEventArgs(
    string regionName,
    Uri fromUri,
    Uri toUri,
    string fromTargetName,
    string toTargetName,
    NavigationMode mode) : EventArgs
{
    /// <summary>
    /// Region name (e.g. "MainRegion").
    /// </summary>
    public string RegionName { get; } = regionName ?? throw new ArgumentNullException(nameof(regionName));

    /// <summary>
    /// URI of the current view before this change (null if none).
    /// </summary>
    public Uri FromUri { get; } = fromUri;

    /// <summary>
    /// Target URI of this navigation.
    /// </summary>
    public Uri ToUri { get; } = toUri ?? throw new ArgumentNullException(nameof(toUri));

    /// <summary>
    /// Target name of the view being left (from FromUri path segment).
    /// </summary>
    public string FromTargetName { get; } = fromTargetName ?? string.Empty;

    /// <summary>
    /// Target name of the view being navigated to (from ToUri path segment).
    /// </summary>
    public string ToTargetName { get; } = toTargetName ?? throw new ArgumentNullException(nameof(toTargetName));

    /// <summary>
    /// How the navigation was initiated: Navigate (Push), Redirect (Replace), GoBack, or GoForward.
    /// </summary>
    public NavigationMode Mode { get; } = mode;

    /// <summary>
    /// Set to true to cancel the navigation. No stack or content change will occur.
    /// </summary>
    public bool Cancel { get; set; } = false;

    public override string ToString()
    {
        return $"{Mode} {RegionName}: {FromTargetName} -> {ToTargetName} ({ToUri})";
    }
}
