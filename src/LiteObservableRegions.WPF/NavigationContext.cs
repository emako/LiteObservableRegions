using System;
using System.Collections.Generic;

namespace LiteObservableRegions;

/// <summary>
/// Read-only context passed to <see cref="INavigationAware"/> and used for journal entries.
/// </summary>
/// <param name="fromUri">URI before this navigation (null for initial).</param>
/// <param name="toUri">Target URI of this navigation (must not be null).</param>
/// <param name="parameters">Parsed query parameters (e.g. from ?a=1&amp;b=2); must not be null.</param>
/// <param name="mode">Navigation mode (Navigate, Redirect, GoBack, GoForward).</param>
/// <param name="regionName">Region name (must not be null).</param>
/// <param name="targetName">Target name from the URI path (must not be null).</param>
public sealed class NavigationContext(Uri fromUri, Uri toUri, IReadOnlyDictionary<string, string> parameters, NavigationMode mode, string regionName, string targetName)
{
    /// <summary>
    /// URI before this navigation (null for initial).
    /// </summary>
    public Uri FromUri { get; } = fromUri;

    /// <summary>
    /// Target URI of this navigation.
    /// </summary>
    public Uri ToUri { get; } = toUri ?? throw new ArgumentNullException(nameof(toUri));

    /// <summary>
    /// Parsed query parameters (e.g. from ?a=1&amp;b=2).
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; } = parameters ?? throw new ArgumentNullException(nameof(parameters));

    /// <summary>
    /// True when navigation was performed via Redirect.
    /// </summary>
    public bool IsRedirect => Mode == NavigationMode.Redirect;

    /// <summary>
    /// Navigation mode for this transition.
    /// </summary>
    public NavigationMode Mode { get; } = mode;

    /// <summary>
    /// Region name (from region://RegionName/TargetName).
    /// </summary>
    public string RegionName { get; } = regionName ?? throw new ArgumentNullException(nameof(regionName));

    /// <summary>
    /// Target name (from region://RegionName/TargetName).
    /// </summary>
    public string TargetName { get; } = targetName ?? throw new ArgumentNullException(nameof(targetName));
}
