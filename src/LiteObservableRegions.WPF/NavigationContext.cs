using System;
using System.Collections.Generic;

namespace LiteObservableRegions;

/// <summary>
/// Navigation mode for the current transition.
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// New navigation (push onto back stack).
    /// </summary>
    Push,

    /// <summary>
    /// Redirect (replace current, no back stack push).
    /// </summary>
    Replace,

    /// <summary>
    /// Navigating back.
    /// </summary>
    Back,

    /// <summary>
    /// Navigating forward.
    /// </summary>
    Forward,
}

/// <summary>
/// Read-only context passed to INavigationAware and used for journal entries.
/// </summary>
public sealed class NavigationContext
{
    /// <summary>
    /// URI before this navigation (null for initial).
    /// </summary>
    public Uri FromUri { get; }

    /// <summary>
    /// Target URI of this navigation.
    /// </summary>
    public Uri ToUri { get; }

    /// <summary>
    /// Parsed query parameters (e.g. from ?a=1&amp;b=2).
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>
    /// True when navigation was performed via Redirect.
    /// </summary>
    public bool IsRedirect => Mode == NavigationMode.Replace;

    /// <summary>
    /// Navigation mode for this transition.
    /// </summary>
    public NavigationMode Mode { get; }

    /// <summary>
    /// Region name (from region://RegionName/TargetName).
    /// </summary>
    public string RegionName { get; }

    /// <summary>
    /// Target name (from region://RegionName/TargetName).
    /// </summary>
    public string TargetName { get; }

    public NavigationContext(
        Uri fromUri,
        Uri toUri,
        IReadOnlyDictionary<string, string> parameters,
        NavigationMode mode,
        string regionName,
        string targetName)
    {
        FromUri = fromUri;
        ToUri = toUri ?? throw new ArgumentNullException(nameof(toUri));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Mode = mode;
        RegionName = regionName ?? throw new ArgumentNullException(nameof(regionName));
        TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
    }
}
