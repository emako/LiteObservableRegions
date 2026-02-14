using System;

namespace LiteObservableRegions;

/// <summary>
/// Static access to the application's service provider. Set by the host (e.g. in App.OnStartup) so that
/// XAML-attached region registration can resolve IRegionManager.
/// </summary>
public static class RegionServiceProvider
{
    /// <summary>
    /// Current service provider. Set by the host after building the DI container.
    /// </summary>
    public static IServiceProvider Current { get; set; }

    /// <summary>
    /// Gets a required service.
    /// </summary>
    public static T GetRequiredService<T>() where T : notnull
    {
        if (Current == null)
            throw new InvalidOperationException("RegionServiceProvider.Current has not been set.");
        object service = Current.GetService(typeof(T));
        return service == null ? throw new InvalidOperationException($"Required service {typeof(T).Name} was not registered.") : (T)service;
    }

    /// <summary>
    /// Gets a service, or null.
    /// </summary>
    public static T GetService<T>()
    {
        if (Current == null)
            return default;
        return (T)Current.GetService(typeof(T));
    }
}
