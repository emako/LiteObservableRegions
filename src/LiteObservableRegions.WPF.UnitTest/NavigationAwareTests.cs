using System;
using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiteObservableRegions.WPF.UnitTest;

/// <summary>
/// Tests for <see cref="INavigationAware"/>: OnNavigatedFrom and OnNavigatedTo are invoked with correct
/// <see cref="NavigationContext"/> when navigating, redirecting, going back, or going forward.
/// </summary>
public sealed class NavigationAwareTests
{
    private static IRegionManager CreateManager()
    {
        var services = new ServiceCollection();
        services.AddRegionViews(reg =>
        {
            reg.AddView<FakeNavigationAwareView>("A", ServiceLifetime.Transient);
            reg.AddView<FakeNavigationAwareView>("B", ServiceLifetime.Transient);
            reg.AddView<FakeNavigationAwareView>("C", ServiceLifetime.Transient);
        });
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            sp.GetService<IRegionHostContentAdapter>()));
        return services.BuildServiceProvider().GetRequiredService<IRegionManager>();
    }

    private static void ClearLog() => FakeNavigationAwareView.CallLog.Clear();

    [Fact]
    public void Navigate_ViewImplementsINavigationAware_OnNavigatedToCalledWithCorrectContext()
    {
        FakeNavigationAwareView.CallLog.Clear();
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());

        manager.Navigate(new Uri("region://R1/A", UriKind.Absolute));

        Assert.Single(FakeNavigationAwareView.CallLog);
        var (kind, ctx) = FakeNavigationAwareView.CallLog[0];
        Assert.Equal("To", kind);
        Assert.Null(ctx.FromUri);
        Assert.NotNull(ctx.ToUri);
        Assert.Contains("/A", ctx.ToUri.ToString());
        Assert.Equal("R1", ctx.RegionName);
        Assert.Equal("A", ctx.TargetName);
        Assert.Equal(NavigationMode.Navigate, ctx.Mode);
        Assert.False(ctx.IsRedirect);
    }

    [Fact]
    public void Navigate_SecondTarget_OnNavigatedFromThenOnNavigatedToWithCorrectContext()
    {
        ClearLog();
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/A", UriKind.Absolute));
        ClearLog();

        manager.Navigate(new Uri("region://R1/B", UriKind.Absolute));

        Assert.Equal(2, FakeNavigationAwareView.CallLog.Count);
        var (kindFrom, ctxFrom) = FakeNavigationAwareView.CallLog[0];
        var (kindTo, ctxTo) = FakeNavigationAwareView.CallLog[1];
        Assert.Equal("From", kindFrom);
        Assert.Equal("To", kindTo);
        // From: context describes the navigation (to B); TargetName is the destination.
        Assert.NotNull(ctxFrom.FromUri);
        Assert.Contains("/A", ctxFrom.FromUri.ToString());
        Assert.NotNull(ctxFrom.ToUri);
        Assert.Contains("/B", ctxFrom.ToUri.ToString());
        Assert.Equal("B", ctxFrom.TargetName);
        Assert.Equal("R1", ctxFrom.RegionName);
        Assert.Equal(NavigationMode.Navigate, ctxFrom.Mode);

        Assert.NotNull(ctxTo.FromUri);
        Assert.Contains("/A", ctxTo.FromUri.ToString());
        Assert.NotNull(ctxTo.ToUri);
        Assert.Contains("/B", ctxTo.ToUri.ToString());
        Assert.Equal("B", ctxTo.TargetName);
        Assert.Equal("R1", ctxTo.RegionName);
        Assert.Equal(NavigationMode.Navigate, ctxTo.Mode);
    }

    [Fact]
    public void Redirect_OnNavigatedFromAndOnNavigatedTo_ContextIsRedirectTrue()
    {
        ClearLog();
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/A", UriKind.Absolute));
        ClearLog();

        manager.Redirect(new Uri("region://R1/B", UriKind.Absolute));

        Assert.Equal(2, FakeNavigationAwareView.CallLog.Count);
        Assert.Equal("From", FakeNavigationAwareView.CallLog[0].Kind);
        Assert.Equal("To", FakeNavigationAwareView.CallLog[1].Kind);
        Assert.True(FakeNavigationAwareView.CallLog[0].Context.IsRedirect);
        Assert.True(FakeNavigationAwareView.CallLog[1].Context.IsRedirect);
        Assert.Equal(NavigationMode.Redirect, FakeNavigationAwareView.CallLog[0].Context.Mode);
        Assert.Equal(NavigationMode.Redirect, FakeNavigationAwareView.CallLog[1].Context.Mode);
    }

    [Fact]
    public void GoBack_OnNavigatedFromAndOnNavigatedToCalledWithGoBackMode()
    {
        ClearLog();
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/A", UriKind.Absolute));
        manager.Navigate(new Uri("region://R1/B", UriKind.Absolute));
        ClearLog();

        manager.GoBack("R1");

        Assert.Equal(2, FakeNavigationAwareView.CallLog.Count);
        Assert.Equal("From", FakeNavigationAwareView.CallLog[0].Kind);
        Assert.Equal("To", FakeNavigationAwareView.CallLog[1].Kind);
        // Leaving B: context has Mode GoBack and destination A.
        Assert.Equal(NavigationMode.GoBack, FakeNavigationAwareView.CallLog[0].Context.Mode);
        Assert.Equal("A", FakeNavigationAwareView.CallLog[0].Context.TargetName);
        // Entering A: receives the stored context from when A was pushed (Mode=Navigate).
        Assert.Equal("A", FakeNavigationAwareView.CallLog[1].Context.TargetName);
    }

    [Fact]
    public void GoForward_OnNavigatedFromAndOnNavigatedToCalledWithGoForwardMode()
    {
        ClearLog();
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/A", UriKind.Absolute));
        manager.Navigate(new Uri("region://R1/B", UriKind.Absolute));
        manager.GoBack("R1");
        ClearLog();

        manager.GoForward("R1");

        Assert.Equal(2, FakeNavigationAwareView.CallLog.Count);
        Assert.Equal("From", FakeNavigationAwareView.CallLog[0].Kind);
        Assert.Equal("To", FakeNavigationAwareView.CallLog[1].Kind);
        // Leaving A: context has Mode GoForward and destination B.
        Assert.Equal(NavigationMode.GoForward, FakeNavigationAwareView.CallLog[0].Context.Mode);
        Assert.Equal("B", FakeNavigationAwareView.CallLog[0].Context.TargetName);
        // Entering B: receives the stored context from when B was pushed (Mode=Navigate).
        Assert.Equal("B", FakeNavigationAwareView.CallLog[1].Context.TargetName);
    }

    [Fact]
    public void Navigate_WithQueryParameters_PassedInContextParameters()
    {
        ClearLog();
        IRegionManager manager = CreateManager();
        manager.RegisterRegion("R1", new FakeHost());

        manager.Navigate(new Uri("region://R1/A?id=1&name=foo", UriKind.Absolute));

        Assert.Single(FakeNavigationAwareView.CallLog);
        var ctx = FakeNavigationAwareView.CallLog[0].Context;
        Assert.NotNull(ctx.Parameters);
        Assert.True(ctx.Parameters.TryGetValue("id", out string? id) && id == "1");
        Assert.True(ctx.Parameters.TryGetValue("name", out string? name) && name == "foo");
    }

    /// <summary>
    /// When the view does not implement INavigationAware but its DataContext does,
    /// OnNavigatedFrom and OnNavigatedTo are invoked on the DataContext.
    /// </summary>
    [StaFact]
    public void Navigate_DataContextImplementsINavigationAware_OnNavigatedToCalledOnDataContext()
    {
        FakeNavigationAwareViewModel.CallLog.Clear();
        var services = new ServiceCollection();
        services.AddRegionViews(reg =>
        {
            reg.AddView<ViewWithAwareDataContext>("D", ServiceLifetime.Transient);
            reg.AddView<FakeNavigationAwareView>("E", ServiceLifetime.Transient);
        });
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            sp.GetService<IRegionHostContentAdapter>()));
        IRegionManager manager = services.BuildServiceProvider().GetRequiredService<IRegionManager>();
        manager.RegisterRegion("R1", new FakeHost());

        manager.Navigate(new Uri("region://R1/D", UriKind.Absolute));

        Assert.Single(FakeNavigationAwareViewModel.CallLog);
        var (kind, ctx) = FakeNavigationAwareViewModel.CallLog[0];
        Assert.Equal("To", kind);
        Assert.Equal("R1", ctx.RegionName);
        Assert.Equal("D", ctx.TargetName);
        Assert.Equal(NavigationMode.Navigate, ctx.Mode);
    }

    /// <summary>
    /// When navigating away from a view whose DataContext is INavigationAware,
    /// OnNavigatedFrom is called on the DataContext; OnNavigatedTo is called on the new view.
    /// </summary>
    [StaFact]
    public void Navigate_FromViewWithAwareDataContext_OnNavigatedFromCalledOnDataContext()
    {
        FakeNavigationAwareViewModel.CallLog.Clear();
        FakeNavigationAwareView.CallLog.Clear();
        var services = new ServiceCollection();
        services.AddRegionViews(reg =>
        {
            reg.AddView<ViewWithAwareDataContext>("D", ServiceLifetime.Transient);
            reg.AddView<FakeNavigationAwareView>("E", ServiceLifetime.Transient);
        });
        services.AddSingleton<IRegionManager>(sp => new RegionManager(
            sp,
            sp.GetRequiredService<IRegionViewRegistry>(),
            sp.GetService<IRegionHostContentAdapter>()));
        IRegionManager manager = services.BuildServiceProvider().GetRequiredService<IRegionManager>();
        manager.RegisterRegion("R1", new FakeHost());
        manager.Navigate(new Uri("region://R1/D", UriKind.Absolute));
        FakeNavigationAwareViewModel.CallLog.Clear();

        manager.Navigate(new Uri("region://R1/E", UriKind.Absolute));

        Assert.Single(FakeNavigationAwareViewModel.CallLog);
        Assert.Equal("From", FakeNavigationAwareViewModel.CallLog[0].Kind);
        Assert.Equal("E", FakeNavigationAwareViewModel.CallLog[0].Context.TargetName);
        Assert.Equal(NavigationMode.Navigate, FakeNavigationAwareViewModel.CallLog[0].Context.Mode);
        Assert.Single(FakeNavigationAwareView.CallLog);
        Assert.Equal("To", FakeNavigationAwareView.CallLog[0].Kind);
    }
}
