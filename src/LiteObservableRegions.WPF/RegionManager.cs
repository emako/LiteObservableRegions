using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

internal sealed class NavigationEntry
{
    public Uri Uri { get; }
    public WeakReference ViewRef { get; }
    public NavigationContext Context { get; }

    public NavigationEntry(Uri uri, object view, NavigationContext context)
    {
        Uri = uri;
        ViewRef = new WeakReference(view);
        Context = context;
    }
}

internal sealed class RegionState
{
    public DependencyObject Host { get; }
    public Stack<NavigationEntry> BackStack { get; } = new Stack<NavigationEntry>();
    public Stack<NavigationEntry> ForwardStack { get; } = new Stack<NavigationEntry>();
    public NavigationEntry CurrentEntry { get; set; }
    public IServiceScope Scope { get; set; }

    public RegionState(DependencyObject host)
    {
        Host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void DisposeScope()
    {
        Scope?.Dispose();
        Scope = null;
    }
}

public sealed class RegionManager : IRegionManager
{
    private readonly IServiceProvider _rootProvider;
    private readonly IRegionViewRegistry _registry;
    private readonly Dictionary<string, RegionState> _regions = new Dictionary<string, RegionState>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object> _singletonCache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public RegionManager(IServiceProvider rootProvider, IRegionViewRegistry registry)
    {
        _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public void RegisterRegion(string regionName, object host)
    {
        if (string.IsNullOrEmpty(regionName)) throw new ArgumentNullException(nameof(regionName));
        if (host == null) throw new ArgumentNullException(nameof(host));

        DependencyObject depObj = host as DependencyObject;
        if (depObj == null)
            throw new ArgumentException("Region host must be a DependencyObject.", nameof(host));

        RegionState existing;
        if (_regions.TryGetValue(regionName, out existing))
        {
            existing.DisposeScope();
        }

        RegionState state = new RegionState(depObj);
        _regions[regionName] = state;

        if (depObj is FrameworkElement fe)
        {
            fe.Unloaded += (s, e) =>
            {
                RegionState st;
                if (_regions.TryGetValue(regionName, out st) && st.Host == depObj)
                {
                    _regions.Remove(regionName);
                    st.DisposeScope();
                }
            };
        }
    }

    public void Navigate(Uri uri)
    {
        PerformNavigation(uri, pushBack: true, clearForward: true);
    }

    public void Redirect(Uri uri)
    {
        PerformNavigation(uri, pushBack: false, clearForward: true);
    }

    public void GoBack(string regionName)
    {
        RegionState state;
        if (!_regions.TryGetValue(regionName, out state))
            return;
        if (state.BackStack.Count == 0)
            return;

        NavigationEntry current = state.CurrentEntry;
        NavigationEntry previous = state.BackStack.Pop();
        state.ForwardStack.Push(new NavigationEntry(current.Uri, GetViewFromEntry(regionName, state, current), current.Context));
        state.CurrentEntry = previous;

        object currentView = GetViewFromEntry(regionName, state, current);
        InvokeNavigatedFrom(currentView, new NavigationContext(current.Context.FromUri, current.Context.ToUri, current.Context.Parameters, NavigationMode.Back, regionName, previous.Context.TargetName));
        object previousView = GetViewFromEntry(regionName, state, previous);
        ObservableRegion.SetHostContent(state.Host, previousView);
        InvokeNavigatedTo(previousView, previous.Context);
    }

    public void GoForward(string regionName)
    {
        RegionState state;
        if (!_regions.TryGetValue(regionName, out state))
            return;
        if (state.ForwardStack.Count == 0)
            return;

        NavigationEntry current = state.CurrentEntry;
        NavigationEntry next = state.ForwardStack.Pop();
        state.BackStack.Push(new NavigationEntry(current.Uri, GetViewFromEntry(regionName, state, current), current.Context));
        state.CurrentEntry = next;

        object currentView = GetViewFromEntry(regionName, state, current);
        InvokeNavigatedFrom(currentView, new NavigationContext(current.Context.FromUri, current.Context.ToUri, current.Context.Parameters, NavigationMode.Forward, regionName, next.Context.TargetName));
        object nextView = GetViewFromEntry(regionName, state, next);
        ObservableRegion.SetHostContent(state.Host, nextView);
        InvokeNavigatedTo(nextView, next.Context);
    }

    public bool CanGoBack(string regionName)
    {
        RegionState state;
        return _regions.TryGetValue(regionName, out state) && state.BackStack.Count > 0;
    }

    public bool CanGoForward(string regionName)
    {
        RegionState state;
        return _regions.TryGetValue(regionName, out state) && state.ForwardStack.Count > 0;
    }

    private void PerformNavigation(Uri uri, bool pushBack, bool clearForward)
    {
        if (uri == null)
            throw new ArgumentNullException(nameof(uri));
        string regionName;
        string targetName;
        IReadOnlyDictionary<string, string> parameters;
        if (!RegionUriParser.TryParse(uri, out regionName, out targetName, out parameters))
            throw new ArgumentException("Invalid region URI. Expected format: region://RegionName/TargetName?query", nameof(uri));

        RegionState state;
        if (!_regions.TryGetValue(regionName, out state))
            throw new InvalidOperationException($"Region '{regionName}' is not registered.");

        NavigationMode mode = pushBack ? NavigationMode.Push : NavigationMode.Replace;
        Uri fromUri = state.CurrentEntry != null ? state.CurrentEntry.Uri : null;
        NavigationContext context = new NavigationContext(fromUri, uri, parameters, mode, regionName, targetName);

        object view = ResolveView(regionName, targetName, parameters, context);
        if (view == null)
            throw new InvalidOperationException($"No view registered for target '{targetName}'.");

        if (state.CurrentEntry != null)
        {
            object currentView = GetViewFromEntry(regionName, state, state.CurrentEntry);
            InvokeNavigatedFrom(currentView, context);
            if (pushBack)
                state.BackStack.Push(state.CurrentEntry);
        }
        if (clearForward)
            state.ForwardStack.Clear();

        state.CurrentEntry = new NavigationEntry(uri, view, context);
        ObservableRegion.SetHostContent(state.Host, view);
        InvokeNavigatedTo(view, context);
    }

    private object ResolveView(string regionName, string targetName, IReadOnlyDictionary<string, string> parameters, NavigationContext context)
    {
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
            object cached;
            if (_singletonCache.TryGetValue(key, out cached))
                return cached;
            object instance = _rootProvider.GetService(entry.ViewType);
            if (instance != null)
                _singletonCache[key] = instance;
            return instance;
        }

        if (lifetime == ServiceLifetime.Scoped)
        {
            RegionState state;
            if (!_regions.TryGetValue(regionName, out state))
                return null;
            if (state.Scope == null)
                state.Scope = _rootProvider.CreateScope();
            return state.Scope.ServiceProvider.GetService(entry.ViewType);
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
        return ResolveView(regionName, entry.Context.TargetName, entry.Context.Parameters, entry.Context);
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
