using System;

namespace LiteObservableRegions;

/// <summary>
/// Provides extension methods for <see cref="IServiceProvider"/> to integrate region service provider functionality.
/// </summary>
public static class RegionServiceProviderExtensions
{
    /// <summary>
    /// Sets the region service provider to the specified <see cref="IServiceProvider"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for region services.</param>
    /// <returns>The same <see cref="IServiceProvider"/> instance provided.</returns>
    public static IServiceProvider UseRegionServiceProvider(this IServiceProvider serviceProvider)
    {
        return WeakReferenceRegionHub.ServiceProvider = serviceProvider;
    }
}
