using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Registry of region target names to view types and lifetimes.
/// Populated via AddRegionViews + AddView; used by RegionManager to resolve views.
/// </summary>
public interface IRegionViewRegistry
{
    /// <summary>
    /// Registers a view type for the given target name and lifetime. Also registers the type in DI.
    /// </summary>
    /// <typeparam name="TView">The view type (class).</typeparam>
    /// <param name="targetName">The target name used in navigation (e.g. "ViewA").</param>
    /// <param name="lifetime">Transient, Scoped (per region), or Singleton (cached per region+target).</param>
    public void AddView<TView>(string targetName, ServiceLifetime lifetime) where TView : class;

    /// <summary>
    /// Gets all registered view entries (target name, view type, lifetime).
    /// </summary>
    /// <returns>Read-only list of view registrations.</returns>
    public IReadOnlyList<ViewRegistration> GetEntries();
}
