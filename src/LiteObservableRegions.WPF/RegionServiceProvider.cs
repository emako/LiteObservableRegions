using System;

namespace LiteObservableRegions;

/// <summary>
/// Static access to the application's service provider. Set by the host (e.g. in App.OnStartup) so that
/// XAML-attached region registration can resolve IRegionManager.
/// </summary>
internal class RegionServiceProvider
{
    /// <summary>
    /// Current service provider. Set by the host after building the DI container.
    /// </summary>
    public IServiceProvider Current { get; set; }

    /// <summary>
    /// Gets a required service. Throws if the provider is not set or the service is not registered.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Current is null or the service was not registered.</exception>
    public T GetRequiredService<T>() where T : notnull
    {
        if (Current == null)
            throw new InvalidOperationException("RegionServiceProvider.Current has not been set.");
        object service = Current.GetService(typeof(T));
        return service == null ? throw new InvalidOperationException($"Required service {typeof(T).Name} was not registered.") : (T)service;
    }

    /// <summary>
    /// Gets a service, or null if the provider is not set or the service is not registered.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance, or default(T) if not available.</returns>
    public T GetService<T>()
    {
        if (Current == null)
            return default;
        return (T)Current.GetService(typeof(T));
    }
}
