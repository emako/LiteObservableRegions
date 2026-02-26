using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1510 // Use 'ArgumentNullException.ThrowIfNull' instead of explicitly throwing a new exception instance

namespace LiteObservableRegions;

/// <summary>
/// Manages region registration (from XAML or code) and per-region navigation (navigate, redirect, back, forward).
/// Resolves views from named views (child elements with <see cref="ObservableRegion.ViewName"/>) or from DI via <see cref="IRegionViewRegistry"/>.
/// </summary>
/// <param name="rootProvider">The root service provider used to resolve views and optional <see cref="IRegionHostContentAdapter"/>.</param>
/// <param name="registry">The view registry (target name to type and lifetime).</param>
/// <param name="contentAdapter">Optional. How to display content in the host; defaults to <see cref="DefaultRegionHostContentAdapter"/>.</param>
/// <param name="onRegionChanging">Optional. Invoked before each navigation (Navigate, Redirect, GoBack, GoForward); used to raise <see cref="WeakReferenceRegionHub.ObservableRegionChanged"/> and allow cancellation.</param>
public sealed class RegionManager(
    IServiceProvider rootProvider,
    IRegionViewRegistry registry,
    IRegionHostContentAdapter contentAdapter = null,
    Action<RegionChangedEventArgs> onRegionChanging = null) : IRegionManager
{
    private readonly IServiceProvider _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
    private readonly IRegionViewRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    private readonly Dictionary<string, RegionState> _regions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object> _singletonCache = new(StringComparer.OrdinalIgnoreCase);

    private readonly IRegionHostContentAdapter _contentAdapter = contentAdapter ?? new DefaultRegionHostContentAdapter();
    private readonly Action<RegionChangedEventArgs> _onRegionChanging = onRegionChanging;

    /// <summary>
    /// All registered regions (region name -> state). Use for direct access to host, stacks, named views, scope.
    /// </summary>
    public Dictionary<string, RegionState> Regions => _regions;

    /// <inheritdoc />
    /// <param name="regionName">Region name; accepts "region://Name" or "Name" (normalized).</param>
    /// <param name="host">The host element (must be a <see cref="DependencyObject"/>). When it is a <see cref="FrameworkElement"/>, unload unregisters the region.</param>
    /// <exception cref="ArgumentException">host is not a DependencyObject.</exception>
    public void RegisterRegion(string regionName, object host)
    {
        if (regionName == null) throw new ArgumentNullException(nameof(regionName));
        regionName = RegionUriParser.NormalizeRegionName(regionName);
        if (string.IsNullOrEmpty(regionName)) throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));
        if (host == null) throw new ArgumentNullException(nameof(host));

        if (host is not DependencyObject depObj)
            throw new ArgumentException("Region host must be a DependencyObject.", nameof(host));

        if (_regions.TryGetValue(regionName, out RegionState existing))
        {
            existing.DisposeScope();
        }

        RegionState state = new(depObj);
        _regions[regionName] = state;

        if (depObj is FrameworkElement fe)
        {
            fe.Unloaded += (s, e) =>
            {
                if (_regions.TryGetValue(regionName, out RegionState st) && st.Host == depObj)
                {
                    _regions.Remove(regionName);
                    st.DisposeScope();
                }
            };
        }
    }

    /// <inheritdoc />
    /// <remarks>If the URI equals the current entry's URI, no operation is performed (no stack or content change).</remarks>
    public void Navigate(Uri uri)
    {
        PerformNavigation(uri, pushBack: true, clearForward: true);
    }

    /// <inheritdoc />
    public void Redirect(Uri uri)
    {
        PerformNavigation(uri, pushBack: false, clearForward: true);
    }

    /// <inheritdoc />
    /// <param name="regionName">The region name (case-insensitive).</param>
    public void GoBack(string regionName)
    {
        if (!_regions.TryGetValue(regionName, out RegionState state))
            return;
        if (state.BackStack.Count == 0)
            return;

        NavigationEntry current = state.CurrentEntry;
        NavigationEntry previous = state.BackStack.Peek();
        RegionChangedEventArgs args = new(
            regionName,
            current?.Uri,
            previous.Uri,
            current?.Context.TargetName ?? string.Empty,
            previous.Context.TargetName,
            NavigationMode.GoBack);
        _onRegionChanging?.Invoke(args);
        if (args.Cancel)
            return;

        state.BackStack.Pop();
        state.ForwardStack.Push(new NavigationEntry(current.Uri, GetViewFromEntry(regionName, state, current), current.Context));
        state.CurrentEntry = previous;

        object currentView = GetViewFromEntry(regionName, state, current);
        InvokeNavigatedFrom(currentView, new NavigationContext(current.Context.FromUri, current.Context.ToUri, current.Context.Parameters, NavigationMode.GoBack, regionName, previous.Context.TargetName));
        object previousView = GetViewFromEntry(regionName, state, previous);
        _contentAdapter.SetContent(state.Host, previousView);
        InvokeNavigatedTo(previousView, previous.Context);
    }

    /// <inheritdoc />
    /// <param name="regionName">The region name (case-insensitive).</param>
    public void GoForward(string regionName)
    {
        if (!_regions.TryGetValue(regionName, out RegionState state))
            return;
        if (state.ForwardStack.Count == 0)
            return;

        NavigationEntry current = state.CurrentEntry;
        NavigationEntry next = state.ForwardStack.Peek();
        RegionChangedEventArgs args = new(
            regionName,
            current?.Uri,
            next.Uri,
            current?.Context.TargetName ?? string.Empty,
            next.Context.TargetName,
            NavigationMode.GoForward);
        _onRegionChanging?.Invoke(args);
        if (args.Cancel)
            return;

        state.ForwardStack.Pop();
        state.BackStack.Push(new NavigationEntry(current.Uri, GetViewFromEntry(regionName, state, current), current.Context));
        state.CurrentEntry = next;

        object currentView = GetViewFromEntry(regionName, state, current);
        InvokeNavigatedFrom(currentView, new NavigationContext(current.Context.FromUri, current.Context.ToUri, current.Context.Parameters, NavigationMode.GoForward, regionName, next.Context.TargetName));
        object nextView = GetViewFromEntry(regionName, state, next);
        _contentAdapter.SetContent(state.Host, nextView);
        InvokeNavigatedTo(nextView, next.Context);
    }

    /// <inheritdoc />
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>True if the region has at least one entry on the back stack.</returns>
    public bool CanGoBack(string regionName)
    {
        return _regions.TryGetValue(regionName, out RegionState state) && state.BackStack.Count > 0;
    }

    /// <inheritdoc />
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>True if the region has at least one entry on the forward stack.</returns>
    public bool CanGoForward(string regionName)
    {
        return _regions.TryGetValue(regionName, out RegionState state) && state.ForwardStack.Count > 0;
    }

    /// <inheritdoc />
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>A read-only view of the region, or null if not registered.</returns>
    public IRegion GetRegion(string regionName)
    {
        if (string.IsNullOrEmpty(regionName))
            return null;
        if (!_regions.TryGetValue(regionName, out _))
            return null;
        return new RegionView(this, regionName);
    }

    /// <inheritdoc />
    /// <param name="regionName">The region name (case-insensitive). Must already be registered.</param>
    /// <param name="viewName">The target name used when navigating to this view.</param>
    /// <param name="view">The view instance. If it is a <see cref="FrameworkElement"/>, unload removes this named view.</param>
    /// <remarks>If the region is not registered, this method does nothing. Re-registering the same viewName overwrites the previous view.</remarks>
    public void RegisterNamedView(string regionName, string viewName, object view)
    {
        if (regionName == null) throw new ArgumentNullException(nameof(regionName));
        regionName = RegionUriParser.NormalizeRegionName(regionName);
        if (string.IsNullOrEmpty(regionName)) throw new ArgumentException("Region name cannot be null or empty.", nameof(regionName));
        if (string.IsNullOrEmpty(viewName)) throw new ArgumentException("View name cannot be null or empty.", nameof(viewName));
        if (view == null) throw new ArgumentNullException(nameof(view));

        if (!_regions.TryGetValue(regionName, out RegionState state))
            return;

        state.NamedViews[viewName] = new WeakReference(view);

        if (view is FrameworkElement fe)
        {
            fe.Unloaded += (s, e) =>
            {
                if (_regions.TryGetValue(regionName, out RegionState st) && st.NamedViews.TryGetValue(viewName, out WeakReference wr) && wr.Target == view)
                {
                    st.NamedViews.Remove(viewName);
                }
            };
        }
    }

    /// <inheritdoc />
    /// <remarks>Does not unregister regions or clear navigation stacks.</remarks>
    public void Clear()
    {
        foreach (RegionState state in _regions.Values)
            state.NamedViews.Clear();
        _singletonCache.Clear();
    }

    private void PerformNavigation(Uri uri, bool pushBack, bool clearForward)
    {
        if (uri == null)
            throw new ArgumentNullException(nameof(uri));
        if (!RegionUriParser.TryParse(uri, out string regionName, out string targetName, out IReadOnlyDictionary<string, string> parameters))
            throw new ArgumentException("Invalid region URI. Expected format: region://RegionName/TargetName?query", nameof(uri));

        if (!_regions.TryGetValue(regionName, out RegionState state))
            throw new InvalidOperationException($"Region '{regionName}' is not registered.");

        // Navigate: avoid redundant navigation to current location (Vue Router style)
        if (pushBack && state.CurrentEntry != null && state.CurrentEntry.Uri != null && state.CurrentEntry.Uri.Equals(uri))
        {
            Debug.WriteLine($"NavigationDuplicated: Avoided redundant navigation to current location ({uri})");
            return;
        }

        NavigationMode mode = pushBack ? NavigationMode.Navigate : NavigationMode.Redirect;
        Uri fromUri = state.CurrentEntry?.Uri;
        string fromTargetName = state.CurrentEntry?.Context.TargetName ?? string.Empty;

        RegionChangedEventArgs args = new(regionName, fromUri, uri, fromTargetName, targetName, mode);
        _onRegionChanging?.Invoke(args);
        if (args.Cancel)
            return;

        NavigationContext context = new(fromUri, uri, parameters, mode, regionName, targetName);

        object view = ResolveView(regionName, targetName) ?? throw new InvalidOperationException($"No view registered for target '{targetName}'.");
        if (state.CurrentEntry != null)
        {
            object currentView = GetViewFromEntry(regionName, state, state.CurrentEntry);
            InvokeNavigatedFrom(currentView, context);
            if (pushBack)
                state.BackStack.Push(state.CurrentEntry);
        }
        if (clearForward)
            state.ForwardStack.Clear();
        if (!pushBack)
            state.BackStack.Clear();

        state.CurrentEntry = new NavigationEntry(uri, view, context);
        _contentAdapter.SetContent(state.Host, view);
        InvokeNavigatedTo(view, context);
    }

    public object ResolveView(string regionName, string targetName)
    {
        if (_regions.TryGetValue(regionName, out RegionState state) &&
            state.NamedViews.TryGetValue(targetName, out WeakReference wr))
        {
            object namedView = wr.Target;
            if (namedView != null)
                return namedView;
            state.NamedViews.Remove(targetName);
        }

        ViewRegistration entry = default;
        foreach (ViewRegistration e in _registry.GetEntries())
        {
            if (string.Equals(e.TargetName, targetName, StringComparison.OrdinalIgnoreCase))
            {
                entry = e;
                break;
            }
        }
        if (entry.ViewType == null)
            return null;

        ServiceLifetime lifetime = entry.Lifetime;
        if (lifetime == ServiceLifetime.Transient)
            return _rootProvider.GetService(entry.ViewType);

        if (lifetime == ServiceLifetime.Singleton)
        {
            string key = regionName + "|" + targetName;
            if (_singletonCache.TryGetValue(key, out object cached))
                return cached;
            object instance = _rootProvider.GetService(entry.ViewType);
            if (instance != null)
                _singletonCache[key] = instance;
            return instance;
        }

        if (lifetime == ServiceLifetime.Scoped)
        {
            if (!_regions.TryGetValue(regionName, out RegionState regionState))
                return null;
            regionState.Scope ??= _rootProvider.CreateScope();
            return regionState.Scope.ServiceProvider.GetService(entry.ViewType);
        }

        return null;
    }

    /// <summary>
    /// Gets the view from an entry; if the weak reference has been collected, re-resolves from DI.
    /// </summary>
    private object GetViewFromEntry(string regionName, RegionState state, NavigationEntry entry)
    {
        object view = entry.ViewRef.Target;
        if (view != null)
            return view;
        return ResolveView(regionName, entry.Context.TargetName);
    }

    private static void InvokeNavigatedFrom(object view, NavigationContext context)
    {
        if (view == null)
            return;
        if (view is INavigationAware aware)
        {
            aware.OnNavigatedFrom(context);
            return;
        }
        if (view is FrameworkElement fe && fe.DataContext is INavigationAware vm)
            vm.OnNavigatedFrom(context);
    }

    private static void InvokeNavigatedTo(object view, NavigationContext context)
    {
        if (view == null)
            return;
        if (view is INavigationAware aware)
        {
            aware.OnNavigatedTo(context);
            return;
        }
        if (view is FrameworkElement fe && fe.DataContext is INavigationAware vm)
            vm.OnNavigatedTo(context);
    }
}

#pragma warning restore CA1510 // Use 'ArgumentNullException.ThrowIfNull' instead of explicitly throwing a new exception instance
#pragma warning restore IDE0079 // Remove unnecessary suppression
