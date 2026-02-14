using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LiteObservableRegions;

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
    private readonly Dictionary<string, RegionState> _regions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object> _singletonCache = new(StringComparer.OrdinalIgnoreCase);

    public RegionManager(IServiceProvider rootProvider, IRegionViewRegistry registry)
    {
        _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

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
        if (!_regions.TryGetValue(regionName, out RegionState state))
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
        if (!_regions.TryGetValue(regionName, out RegionState state))
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
        return _regions.TryGetValue(regionName, out RegionState state) && state.BackStack.Count > 0;
    }

    public bool CanGoForward(string regionName)
    {
        return _regions.TryGetValue(regionName, out RegionState state) && state.ForwardStack.Count > 0;
    }

    public IRegion GetRegion(string regionName)
    {
        if (string.IsNullOrEmpty(regionName))
            return null;
        if (!_regions.TryGetValue(regionName, out RegionState state))
            return null;
        return new RegionView(this, regionName);
    }

    private sealed class RegionView : IRegion
    {
        private readonly RegionManager _manager;
        private readonly string _name;

        internal RegionView(RegionManager manager, string name)
        {
            _manager = manager;
            _name = name;
        }

        public string Name => _name;

        public object Host
        {
            get
            {
                RegionState state;
                return _manager._regions.TryGetValue(_name, out state) ? state.Host : null;
            }
        }

        public Uri CurrentUri
        {
            get
            {
                if (!_manager._regions.TryGetValue(_name, out RegionState state) || state.CurrentEntry == null)
                    return null;
                return state.CurrentEntry.Uri;
            }
        }

        public bool CanGoBack => _manager.CanGoBack(_name);
        public bool CanGoForward => _manager.CanGoForward(_name);
    }

    private void PerformNavigation(Uri uri, bool pushBack, bool clearForward)
    {
        if (uri == null)
            throw new ArgumentNullException(nameof(uri));
        if (!RegionUriParser.TryParse(uri, out string regionName, out string targetName, out IReadOnlyDictionary<string, string> parameters))
            throw new ArgumentException("Invalid region URI. Expected format: region://RegionName/TargetName?query", nameof(uri));

        if (!_regions.TryGetValue(regionName, out RegionState state))
            throw new InvalidOperationException($"Region '{regionName}' is not registered.");

        NavigationMode mode = pushBack ? NavigationMode.Push : NavigationMode.Replace;
        Uri fromUri = state.CurrentEntry?.Uri;
        NavigationContext context = new(fromUri, uri, parameters, mode, regionName, targetName);

        object view = ResolveView(regionName, targetName, parameters, context) ?? throw new InvalidOperationException($"No view registered for target '{targetName}'.");
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
            if (_singletonCache.TryGetValue(key, out object cached))
                return cached;
            object instance = _rootProvider.GetService(entry.ViewType);
            if (instance != null)
                _singletonCache[key] = instance;
            return instance;
        }

        if (lifetime == ServiceLifetime.Scoped)
        {
            if (!_regions.TryGetValue(regionName, out RegionState state))
                return null;
            state.Scope ??= _rootProvider.CreateScope();
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
