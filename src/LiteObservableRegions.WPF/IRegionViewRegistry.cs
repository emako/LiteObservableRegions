using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

/// <summary>
/// Registry of region target names to view types and lifetimes.
/// Populated via AddRegionViews + AddView; used by RegionManager to resolve views.
/// </summary>
public interface IRegionViewRegistry
{
    /// <summary>
    /// Registers a view type for the given target name and lifetime. Also registers the type in DI.
    /// </summary>
    public void AddView<TView>(string targetName, ServiceLifetime lifetime) where TView : class;

    /// <summary>
    /// Gets all registered view entries (target name, view type, lifetime).
    /// </summary>
    public IReadOnlyList<ViewRegistration> GetEntries();
}
