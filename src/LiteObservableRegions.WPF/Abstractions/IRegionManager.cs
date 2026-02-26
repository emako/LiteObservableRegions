using System.Collections.Generic;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Manages region registration (from XAML) and delegates navigation to per-region stacks.
/// </summary>
public interface IRegionManager : IRegionNavigation
{
    /// <summary>
    /// All registered regions (region name -> state). Use for direct access to named views, stacks, host, etc.
    /// </summary>
    public Dictionary<string, RegionState> Regions { get; }

    /// <summary>
    /// Registers a host element as a region. Called by the ObservableRegion attached property.
    /// </summary>
    public void RegisterRegion(string regionName, object host);

    /// <summary>
    /// Gets a region by name; returns null if the region is not registered.
    /// </summary>
    public IRegion GetRegion(string regionName);

    /// <summary>
    /// Registers a named view for a region. When navigating to this view name, the given view instance is used instead of resolving via DI.
    /// Typically called by the ObservableRegion.ViewName attached property when parsing e.g. region://TestGridRegion/View1.
    /// </summary>
    public void RegisterNamedView(string regionName, string viewName, object view);

    /// <summary>
    /// Clears all named views (per region) and the singleton view cache. Does not unregister regions or navigation stacks.
    /// </summary>
    public void Clear();
}
