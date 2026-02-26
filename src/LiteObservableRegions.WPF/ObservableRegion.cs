using LiteObservableRegions.Abstractions;
using System;
using System.Windows;

namespace LiteObservableRegions;

/// <summary>
/// Attached properties for declaring a region in XAML and binding current content.
/// </summary>
public static class ObservableRegion
{
    /// <summary>
    /// Region name (e.g. "MainRegion"). Set on the host element.
    /// </summary>
    public static readonly DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached(
        "RegionName",
        typeof(string),
        typeof(ObservableRegion),
        new PropertyMetadata(null, OnRegionNameChanged));

    public static string GetRegionName(DependencyObject obj)
    {
        return (string)obj.GetValue(RegionNameProperty);
    }

    public static void SetRegionName(DependencyObject obj, string value)
    {
        obj.SetValue(RegionNameProperty, value);
    }

    /// <summary>
    /// View name URI (e.g. "region://TestGridRegion/View1"). Set on a child of the region host.
    /// The host part must match the host's RegionName; the path segment (e.g. View1) is used to switch views without DI.
    /// </summary>
    public static readonly DependencyProperty ViewNameProperty = DependencyProperty.RegisterAttached(
        "ViewName",
        typeof(string),
        typeof(ObservableRegion),
        new PropertyMetadata(null, OnViewNameChanged));

    public static string GetViewName(DependencyObject obj)
    {
        return (string)obj.GetValue(ViewNameProperty);
    }

    public static void SetViewName(DependencyObject obj, string value)
    {
        obj.SetValue(ViewNameProperty, value);
    }

    private static void OnViewNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        string value = e.NewValue as string;
        if (string.IsNullOrEmpty(value))
            return;
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri) || !RegionUriParser.TryParse(uri, out string regionName, out string viewName, out _))
            return;
        if (string.IsNullOrEmpty(viewName))
            return;

        DependencyObject host = FindRegionHost(d);
        if (host == null)
            return;
        string hostRegionName = RegionUriParser.NormalizeRegionName(GetRegionName(host) ?? string.Empty);
        if (string.IsNullOrEmpty(hostRegionName) || !string.Equals(hostRegionName, regionName, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            IServiceProvider provider = WeakReferenceRegionHub.ServiceProvider;
            if (provider != null)
            {
                IRegionManager manager = provider.GetService(typeof(IRegionManager)) as IRegionManager;
                manager?.RegisterNamedView(hostRegionName, viewName, d);
            }
        }
        catch
        {
            // RegionServiceProvider may not be set yet.
        }
    }

    /// <summary>
    /// Finds the nearest ancestor that has RegionName set (the region host).
    /// </summary>
    private static DependencyObject FindRegionHost(DependencyObject child)
    {
        DependencyObject current = child;
        while (current != null)
        {
            string name = GetRegionName(current);
            if (!string.IsNullOrEmpty(name))
                return current;
            current = LogicalTreeHelper.GetParent(current);
        }
        return null;
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        string name = e.NewValue as string;
        if (string.IsNullOrEmpty(name))
            return;
        string regionName = RegionUriParser.NormalizeRegionName(name);
        if (string.IsNullOrEmpty(regionName))
            return;
        if (regionName != name)
        {
            SetRegionName(d, regionName);
            return;
        }
        try
        {
            IServiceProvider provider = WeakReferenceRegionHub.ServiceProvider;
            if (provider != null)
            {
                IRegionManager manager = provider.GetService(typeof(IRegionManager)) as IRegionManager;
                manager?.RegisterRegion(name, d);
            }
        }
        catch
        {
            // RegionServiceProvider may not be set yet (e.g. during XAML load before app startup).
        }
    }

    /// <summary>
    /// Current content (view) shown in the region. Set by RegionManager; can be bound.
    /// </summary>
    public static readonly DependencyProperty CurrentContentProperty = DependencyProperty.RegisterAttached(
        "CurrentContent",
        typeof(object),
        typeof(ObservableRegion),
        new PropertyMetadata(null));

    public static object GetCurrentContent(DependencyObject obj)
    {
        return obj.GetValue(CurrentContentProperty);
    }

    public static void SetCurrentContent(DependencyObject obj, object value)
    {
        obj.SetValue(CurrentContentProperty, value);
    }

    /// <summary>
    /// Region context data. No change handling; use as storage for arbitrary object data.
    /// </summary>
    public static readonly DependencyProperty RegionContextProperty = DependencyProperty.RegisterAttached(
        "RegionContext",
        typeof(object),
        typeof(ObservableRegion),
        new PropertyMetadata(null));

    public static object GetRegionContext(DependencyObject obj)
    {
        return obj.GetValue(RegionContextProperty);
    }

    public static void SetRegionContext(DependencyObject obj, object value)
    {
        obj.SetValue(RegionContextProperty, value);
    }
}
