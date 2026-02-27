using LiteObservableRegions.Abstractions;
using System;
using System.Windows;

namespace LiteObservableRegions;

/// <summary>
/// Attached properties for declaring a region in XAML and binding current content.
/// Set <see cref="RegionNameProperty"/> on the host element; optionally set <see cref="ViewNameProperty"/> on children for named views.
/// </summary>
public static class ObservableRegion
{
    /// <summary>
    /// Region name (e.g. "MainRegion"). Set on the host element to register it as a region.
    /// </summary>
    public static readonly DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached(
        "RegionName",
        typeof(string),
        typeof(ObservableRegion),
        new PropertyMetadata(null, OnRegionNameChanged));

    /// <summary>
    /// Gets the region name from the given element.
    /// </summary>
    /// <param name="obj">The element.</param>
    /// <returns>The region name, or null if not set.</returns>
    public static string GetRegionName(DependencyObject obj)
    {
        return (string)obj.GetValue(RegionNameProperty);
    }

    /// <summary>
    /// Sets the region name on the given element (registers it as a region host).
    /// </summary>
    /// <param name="obj">The host element.</param>
    /// <param name="value">Region name; "region://Name" or "Name" (normalized on set).</param>
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

    /// <summary>
    /// Gets the view name URI from the given element (e.g. "region://RegionName/View1").
    /// </summary>
    /// <param name="obj">The element (typically a child of the region host).</param>
    /// <returns>The view name URI, or null if not set.</returns>
    public static string GetViewName(DependencyObject obj)
    {
        return (string)obj.GetValue(ViewNameProperty);
    }

    /// <summary>
    /// Sets the view name URI on a child of the host; the child is registered as a named view for the path segment.
    /// </summary>
    /// <param name="obj">The child element.</param>
    /// <param name="value">Full URI like "region://RegionName/View1"; host part must match the host's RegionName.</param>
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

        try
        {
            IServiceProvider provider = WeakReferenceRegionHub.ServiceProvider;
            if (provider != null)
            {
                IRegionManager manager = provider.GetService(typeof(IRegionManager)) as IRegionManager;
                manager?.RegisterNamedView(regionName, viewName, d);
            }
        }
        catch
        {
            // RegionServiceProvider may not be set yet.
        }
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

    /// <summary>
    /// Gets the current content (view) displayed in the region from the given host.
    /// </summary>
    /// <param name="obj">The region host element.</param>
    /// <returns>The current view instance, or null.</returns>
    public static object GetCurrentContent(DependencyObject obj)
    {
        return obj.GetValue(CurrentContentProperty);
    }

    /// <summary>
    /// Sets the current content on the host (used by the library; can be used for binding or custom adapters).
    /// </summary>
    /// <param name="obj">The region host element.</param>
    /// <param name="value">The view to display.</param>
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

    /// <summary>
    /// Gets the region context value from the given element.
    /// </summary>
    /// <param name="obj">The element.</param>
    /// <returns>The stored context object, or null.</returns>
    public static object GetRegionContext(DependencyObject obj)
    {
        return obj.GetValue(RegionContextProperty);
    }

    /// <summary>
    /// Sets the region context on the given element (arbitrary data; no change notifications).
    /// </summary>
    /// <param name="obj">The element.</param>
    /// <param name="value">The context value.</param>
    public static void SetRegionContext(DependencyObject obj, object value)
    {
        obj.SetValue(RegionContextProperty, value);
    }
}
