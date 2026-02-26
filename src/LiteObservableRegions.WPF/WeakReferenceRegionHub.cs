using LiteObservableRegions.Abstractions;
using System;

namespace LiteObservableRegions;

public class WeakReferenceRegionHub
{
    public static IServiceProvider ServiceProvider
    {
        get => RegionServiceProvider.Current;
        set => RegionServiceProvider.Current = value;
    }

    public static IRegionManager RegionManager
        => RegionServiceProvider.GetRequiredService<IRegionManager>();
}
