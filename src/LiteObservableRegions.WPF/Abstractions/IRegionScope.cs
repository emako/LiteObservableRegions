using System.Windows;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Marker interface for region host or named view elements.
/// When a <see cref="FrameworkElement"/> implements this interface,
/// the region (or named view) is unregistered when the element raises Unloaded.
/// If the element does not implement IRegionScope, the region remains registered after Unloaded
/// (e.g. when the host is inside TabControl or lazy-loaded content).
/// </summary>
public interface IRegionScope
{
    /// <summary>
    /// When true, the region scope is disposed and unregistered when the host or named view raises Unloaded.
    /// When false, the region remains registered after Unloaded (e.g. when the host is inside TabControl or lazy-loaded content).
    /// </summary>
    public bool DisposeRegionScopeOnUnload { get; set; }
}
