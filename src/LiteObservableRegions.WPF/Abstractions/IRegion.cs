using System;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Read-only view of a registered region (host, current URI, navigation state).
/// </summary>
public interface IRegion
{
    /// <summary>
    /// Region name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Host element (DependencyObject) that displays the region content.
    /// </summary>
    public object Host { get; }

    /// <summary>
    /// URI of the current view; null if no navigation has occurred yet.
    /// </summary>
    public Uri CurrentUri { get; }

    /// <summary>
    /// Whether this region can go back.
    /// </summary>
    public bool CanGoBack { get; }

    /// <summary>
    /// Whether this region can go forward.
    /// </summary>
    public bool CanGoForward { get; }
}
