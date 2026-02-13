using System;

namespace LiteObservableRegions;

/// <summary>
/// Manages region registration (from XAML) and delegates navigation to per-region stacks.
/// </summary>
public interface IRegionManager : IRegionNavigation
{
    /// <summary>
    /// Registers a host element as a region. Called by the ObservableRegion attached property.
    /// </summary>
    public void RegisterRegion(string regionName, object host);
}
