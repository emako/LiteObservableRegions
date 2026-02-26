using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LiteObservableRegions;

internal sealed class RegionState(DependencyObject host)
{
    public DependencyObject Host { get; } = host ?? throw new ArgumentNullException(nameof(host));
    public Stack<NavigationEntry> BackStack { get; } = new Stack<NavigationEntry>();
    public Stack<NavigationEntry> ForwardStack { get; } = new Stack<NavigationEntry>();
    public NavigationEntry CurrentEntry { get; set; }
    public IServiceScope Scope { get; set; }

    public void DisposeScope()
    {
        Scope?.Dispose();
        Scope = null;
    }
}
