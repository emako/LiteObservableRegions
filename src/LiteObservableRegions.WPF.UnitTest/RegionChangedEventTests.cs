using System;
using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiteObservableRegions.WPF.UnitTest;

public sealed class RegionChangedEventTests
{
    /// <summary>
    /// Creates a manager with an optional callback; when not provided, uses AddObservableRegions (wires hub event).
    /// </summary>
    private static IRegionManager CreateManager(Action<RegionChangedEventArgs>? onRegionChanging = null)
    {
        IServiceCollection services = new ServiceCollection();
        if (onRegionChanging != null)
        {
            services.AddRegionViews(reg =>
            {
                reg.AddView<FakeView>("GridA", ServiceLifetime.Transient);
                reg.AddView<FakeView>("GridB", ServiceLifetime.Transient);
            });
            services.AddSingleton<IRegionManager>(sp => new RegionManager(
                sp,
                sp.GetRequiredService<IRegionViewRegistry>(),
                sp.GetService<IRegionHostContentAdapter>(),
                onRegionChanging));
        }
        else
        {
            services.AddObservableRegions(reg =>
            {
                reg.AddView<FakeView>("GridA", ServiceLifetime.Transient);
                reg.AddView<FakeView>("GridB", ServiceLifetime.Transient);
            });
        }
        IServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IRegionManager>();
    }

    [Fact]
    public void ObservableRegionChanged_WhenNavigate_FiresWithCorrectArgs()
    {
        RegionChangedEventArgs? captured = null;
        IRegionManager manager = CreateManager(e => captured = e);
        manager.RegisterRegion("MainGridRegion", new FakeHost());

        manager.Navigate(new Uri("region://MainGridRegion/GridB", UriKind.Absolute));

        Assert.NotNull(captured);
        // RegionName in args preserves the casing of the registered region name.
        Assert.Equal("MainGridRegion", captured.RegionName);
        Assert.Null(captured.FromUri);
        Assert.NotNull(captured.ToUri);
        Assert.Contains("GridB", captured.ToUri.ToString());
        Assert.Equal("", captured.FromTargetName);
        Assert.Equal("GridB", captured.ToTargetName);
        Assert.Equal(NavigationMode.Navigate, captured.Mode);
        Assert.False(captured.Cancel);
    }

    [Fact]
    public void ObservableRegionChanged_WhenRedirect_FiresWithReplaceMode()
    {
        RegionChangedEventArgs? captured = null;
        IRegionManager manager = CreateManager(e => captured = e);
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/GridA", UriKind.Absolute));

        manager.Redirect(new Uri("region://R1/GridB", UriKind.Absolute));

        Assert.NotNull(captured);
        Assert.Equal("R1", captured.RegionName);
        Assert.Equal("GridA", captured.FromTargetName);
        Assert.Equal("GridB", captured.ToTargetName);
        Assert.Equal(NavigationMode.Redirect, captured.Mode);
    }

    [Fact]
    public void ObservableRegionChanged_WhenGoBack_FiresWithBackMode()
    {
        RegionChangedEventArgs? captured = null;
        IRegionManager manager = CreateManager(e => captured = e);
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/GridA", UriKind.Absolute));
        manager.Navigate(new Uri("region://R1/GridB", UriKind.Absolute));

        manager.GoBack("R1");

        Assert.NotNull(captured);
        Assert.Equal("R1", captured.RegionName);
        Assert.Equal("GridB", captured.FromTargetName);
        Assert.Equal("GridA", captured.ToTargetName);
        Assert.Equal(NavigationMode.GoBack, captured.Mode);
    }

    [Fact]
    public void ObservableRegionChanged_WhenGoForward_FiresWithForwardMode()
    {
        RegionChangedEventArgs? captured = null;
        IRegionManager manager = CreateManager(e => captured = e);
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/GridA", UriKind.Absolute));
        manager.Navigate(new Uri("region://R1/GridB", UriKind.Absolute));
        manager.GoBack("R1");

        manager.GoForward("R1");

        Assert.NotNull(captured);
        Assert.Equal("R1", captured.RegionName);
        Assert.Equal("GridA", captured.FromTargetName);
        Assert.Equal("GridB", captured.ToTargetName);
        Assert.Equal(NavigationMode.GoForward, captured.Mode);
    }

    [Fact]
    public void ObservableRegionChanged_WhenCancelTrue_NavigateDoesNotOccur()
    {
        IRegionManager manager = CreateManager(e =>
        {
            if (e.ToTargetName == "GridB")
                e.Cancel = true;
        });
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/GridA", UriKind.Absolute));

        manager.Navigate(new Uri("region://R1/GridB", UriKind.Absolute));

        IRegion region = manager.GetRegion("R1");
        Assert.NotNull(region?.CurrentUri);
        Assert.Contains("GridA", region.CurrentUri.ToString());
    }

    [Fact]
    public void ObservableRegionChanged_WhenCancelTrue_GoBackDoesNotOccur()
    {
        IRegionManager manager = CreateManager(e =>
        {
            if (e.Mode == NavigationMode.GoBack)
                e.Cancel = true;
        });
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/GridA", UriKind.Absolute));
        manager.Navigate(new Uri("region://R1/GridB", UriKind.Absolute));

        manager.GoBack("R1");

        IRegion region = manager.GetRegion("R1");
        Assert.NotNull(region?.CurrentUri);
        Assert.Contains("GridB", region.CurrentUri.ToString());
    }

    // -------------------------------------------------------------------------
    // CanGo tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a manager wired through AddObservableRegions (hub event connected).
    /// </summary>
    private static IRegionManager CreateManagerViaHub()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddObservableRegions(reg =>
        {
            reg.AddView<FakeView>("GridA", ServiceLifetime.Transient);
            reg.AddView<FakeView>("GridB", ServiceLifetime.Transient);
        });
        IServiceProvider provider = services.BuildServiceProvider();
        WeakReferenceRegionHub.ServiceProvider = provider;
        return provider.GetRequiredService<IRegionManager>();
    }

    [Fact]
    public void CanGo_WithNoSubscribers_ReturnsTrue()
    {
        IRegionManager manager = CreateManagerViaHub();
        // Ensure no leftover subscribers from other tests by using a fresh hub path via direct RegionManager
        IServiceCollection services = new ServiceCollection();
        services.AddRegionViews(reg => reg.AddView<FakeView>("GridA", ServiceLifetime.Transient));
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>()));
        IRegionManager mgr = services.BuildServiceProvider().GetRequiredService<IRegionManager>();

        bool result = mgr.CanGo(new Uri("region://AnyRegion/GridA", UriKind.Absolute));

        Assert.True(result);
    }

    [Fact]
    public void CanGo_WhenSubscriberAllows_ReturnsTrue()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRegionViews(reg => reg.AddView<FakeView>("GridA", ServiceLifetime.Transient));
        RegionEventNotifier notifier = new();
        notifier.ObservableRegionCanGo += (s, e) => { /* do not veto */ };
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            onCanGo: notifier.RaiseCanGo));
        IRegionManager mgr = services.BuildServiceProvider().GetRequiredService<IRegionManager>();

        bool result = mgr.CanGo(new Uri("region://AnyRegion/GridA", UriKind.Absolute));

        Assert.True(result);
    }

    [Fact]
    public void CanGo_WhenSubscriberVetoes_ReturnsFalse()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRegionViews(reg => reg.AddView<FakeView>("GridA", ServiceLifetime.Transient));
        RegionEventNotifier notifier = new();
        notifier.ObservableRegionCanGo += (s, e) => e.CanGo = false;
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            onCanGo: notifier.RaiseCanGo));
        IRegionManager mgr = services.BuildServiceProvider().GetRequiredService<IRegionManager>();

        bool result = mgr.CanGo(new Uri("region://AnyRegion/GridA", UriKind.Absolute));

        Assert.False(result);
    }

    [Fact]
    public void CanGo_VetoDoesNotPreventNavigate()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRegionViews(reg =>
        {
            reg.AddView<FakeView>("GridA", ServiceLifetime.Transient);
            reg.AddView<FakeView>("GridB", ServiceLifetime.Transient);
        });
        RegionEventNotifier notifier = new();
        notifier.ObservableRegionCanGo += (s, e) => e.CanGo = false;
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            onCanGo: notifier.RaiseCanGo));
        IRegionManager mgr = services.BuildServiceProvider().GetRequiredService<IRegionManager>();
        mgr.RegisterRegion("R1", new FakeHost());
        mgr.Navigate(new Uri("region://R1/GridA", UriKind.Absolute));

        // CanGo vetoes, but Navigate should still succeed
        bool canGo = mgr.CanGo(new Uri("region://R1/GridB", UriKind.Absolute));
        mgr.Navigate(new Uri("region://R1/GridB", UriKind.Absolute));

        Assert.False(canGo);
        IRegion region = mgr.GetRegion("R1");
        Assert.Contains("GridB", region.CurrentUri.ToString());
    }

    [Fact]
    public void CanGo_EventArgsExposesCorrectUri()
    {
        Uri queried = new("region://MainRegion/GridA", UriKind.Absolute);
        Uri? captured = null;
        IServiceCollection services = new ServiceCollection();
        services.AddRegionViews(reg => reg.AddView<FakeView>("GridA", ServiceLifetime.Transient));
        RegionEventNotifier notifier = new();
        notifier.ObservableRegionCanGo += (s, e) => captured = e.Uri;
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            onCanGo: notifier.RaiseCanGo));
        IRegionManager mgr = services.BuildServiceProvider().GetRequiredService<IRegionManager>();

        mgr.CanGo(queried);

        Assert.Equal(queried, captured);
    }

    [Fact]
    public void CanGo_NullUri_Throws()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddRegionViews(reg => reg.AddView<FakeView>("GridA", ServiceLifetime.Transient));
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>()));
        IRegionManager mgr = services.BuildServiceProvider().GetRequiredService<IRegionManager>();

        Assert.Throws<ArgumentNullException>(() => mgr.CanGo(null!));
    }
}
