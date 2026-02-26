using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LiteObservableRegions;

public sealed class RegionState(DependencyObject host)
{
    public DependencyObject Host { get; } = host ?? throw new ArgumentNullException(nameof(host));
    public Stack<NavigationEntry> BackStack { get; } = new Stack<NavigationEntry>();
    public Stack<NavigationEntry> ForwardStack { get; } = new Stack<NavigationEntry>();
    public NavigationEntry CurrentEntry { get; set; }
    public IServiceScope Scope { get; set; }

    /// <summary>
    /// View name -> view instance (weak). Filled by RegisterNamedView (e.g. from ObservableRegion.ViewName).
    /// </summary>
    public Dictionary<string, WeakReference> NamedViews { get; } = new Dictionary<string, WeakReference>(StringComparer.OrdinalIgnoreCase);

    public void DisposeScope()
    {
        Scope?.Dispose();
        Scope = null;
    }
}
