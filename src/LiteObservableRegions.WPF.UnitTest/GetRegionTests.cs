using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiteObservableRegions.WPF.UnitTest;

public sealed class GetRegionTests
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
    public void GetRegion_WhenRegionNotRegistered_ReturnsNull()
    {
        IRegionManager manager = CreateManager();
        IRegion region = manager.GetRegion("NonExistent");
        Assert.Null(region);
    }

    [Fact]
    public void GetRegion_WhenRegionNameIsNull_ReturnsNull()
    {
        IRegionManager manager = CreateManager();
        IRegion region = manager.GetRegion(null);
        Assert.Null(region);
    }

    [Fact]
    public void GetRegion_WhenRegionNameIsEmpty_ReturnsNull()
    {
        IRegionManager manager = CreateManager();
        IRegion region = manager.GetRegion("");
        Assert.Null(region);
    }

    [Fact]
    public void GetRegion_AfterRegisterRegion_ReturnsRegionWithCorrectNameAndHost()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("MainGridRegion", host);

        IRegion region = manager.GetRegion("MainGridRegion");
        Assert.NotNull(region);
        Assert.Equal("MainGridRegion", region.Name);
        Assert.Same(host, region.Host);
    }

    [Fact]
    public void GetRegion_WhenNoNavigationYet_CurrentUriIsNull_CanGoBackAndCanGoForwardAreFalse()
    {
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());

        IRegion region = manager.GetRegion("R1");
        Assert.NotNull(region);
        Assert.Null(region.CurrentUri);
        Assert.False(region.CanGoBack);
        Assert.False(region.CanGoForward);
    }

    [Fact]
    public void GetRegion_AfterNavigate_CurrentUriIsSet_CanGoBackIsTrue()
    {
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("MainGridRegion", new FakeHost());
        manager.Navigate(new Uri("region://MainGridRegion/GridA", UriKind.Absolute));
        manager.Navigate(new Uri("region://MainGridRegion/GridB", UriKind.Absolute));

        IRegion region = manager.GetRegion("MainGridRegion");
        Assert.NotNull(region);
        Assert.NotNull(region.CurrentUri);
        Assert.Contains("GridB", region.CurrentUri.ToString());
        Assert.True(region.CanGoBack);
        Assert.False(region.CanGoForward);
    }

    [Fact]
    public void RegisterRegion_AcceptsRegionUri_AndStoresNormalizedName()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("region://MainGridRegion", host);

        IRegion region = manager.GetRegion("MainGridRegion");
        Assert.NotNull(region);
        Assert.Equal("MainGridRegion", region.Name);
        Assert.Same(host, region.Host);
    }

    [Fact]
    public void GetRegion_IsCaseInsensitive()
    {
        IRegionManager manager = CreateManager();
        FakeHost host = new();
        manager.RegisterRegion("MainGridRegion", host);

        IRegion lower = manager.GetRegion("maingridregion");
        IRegion mixed = manager.GetRegion("MainGridRegion");
        Assert.NotNull(lower);
        Assert.NotNull(mixed);
        Assert.Same(host, lower.Host);
        Assert.Same(lower.Host, mixed.Host);
    }
}
