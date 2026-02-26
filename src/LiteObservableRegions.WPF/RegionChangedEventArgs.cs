using System;
using System.Diagnostics;

namespace LiteObservableRegions;

/// <summary>
/// Event arguments for region content change (before the change is applied).
/// Subscribers can set <see cref="Cancel"/> to true to prevent the navigation.
/// </summary>
/// <param name="regionName">The region name (must not be null).</param>
/// <param name="fromUri">URI of the current view before this change (null if none).</param>
/// <param name="toUri">Target URI of this navigation (must not be null).</param>
/// <param name="fromTargetName">Target name of the view being left.</param>
/// <param name="toTargetName">Target name of the view being navigated to (must not be null).</param>
/// <param name="mode">How the navigation was initiated: Navigate, Redirect, GoBack, or GoForward.</param>
[DebuggerDisplay("{ToString()}")]
public sealed class RegionChangedEventArgs(string regionName, Uri fromUri, Uri toUri, string fromTargetName, string toTargetName, NavigationMode mode) : EventArgs
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

    /// <inheritdoc />
    /// <returns>A string like "Navigate MainRegion: ViewA -> ViewB (region://MainRegion/ViewB)".</returns>
    public override string ToString()
    {
        return $"{Mode} {RegionName}: {FromTargetName} -> {ToTargetName} ({ToUri?.OriginalString ?? ToUri?.ToString()})";
    }
}
