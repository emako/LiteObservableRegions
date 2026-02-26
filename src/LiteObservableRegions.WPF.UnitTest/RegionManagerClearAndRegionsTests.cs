using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiteObservableRegions.WPF.UnitTest;

public sealed class RegionManagerClearAndRegionsTests
{
    private static IRegionManager CreateManager()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddObservableRegions(reg =>
        {
            reg.AddView<FakeView>("GridA", ServiceLifetime.Transient);
            reg.AddView<FakeView>("GridB", ServiceLifetime.Transient);
        });
        IServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IRegionManager>();
    }

    [Fact]
    public void Regions_AfterRegisterRegion_ContainsRegionWithCorrectHost()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("MainGridRegion", host);

        Assert.True(manager.Regions.ContainsKey("MainGridRegion"));
        RegionState state = manager.Regions["MainGridRegion"];
        Assert.NotNull(state);
        Assert.Same(host, state.Host);
    }

    [Fact]
    public void Regions_IsCaseInsensitive()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("MainGridRegion", host);

        Assert.True(manager.Regions.ContainsKey("maingridregion"));
        Assert.Same(manager.Regions["MainGridRegion"], manager.Regions["maingridregion"]);
    }

    [Fact]
    public void Clear_RemovesNamedViewsFromAllRegions()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("R1", host);
        object namedView = new();
        manager.RegisterNamedView("R1", "V1", namedView);

        Assert.Single(manager.Regions["R1"].NamedViews);
        Assert.True(manager.Regions["R1"].NamedViews.ContainsKey("V1"));

        manager.Clear();

        Assert.Empty(manager.Regions["R1"].NamedViews);
    }

    [Fact]
    public void Clear_DoesNotUnregisterRegions()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("R1", host);
        manager.RegisterNamedView("R1", "V1", new());

        manager.Clear();

        Assert.True(manager.Regions.ContainsKey("R1"));
        Assert.Same(host, manager.Regions["R1"].Host);
    }
}
