using System;
using System.Windows;
using System.Windows.Controls;

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

    /// <summary>
    /// Current content (view) shown in the region. Set by RegionManager; can be bound.
    /// </summary>
    public static readonly DependencyProperty CurrentContentProperty = DependencyProperty.RegisterAttached(
        "CurrentContent",
        typeof(object),
        typeof(ObservableRegion),
        new PropertyMetadata(null));

    public static string GetRegionName(DependencyObject obj)
    {
        return (string)obj.GetValue(RegionNameProperty);
    }

    public static void SetRegionName(DependencyObject obj, string value)
    {
        obj.SetValue(RegionNameProperty, value);
    }

    public static object GetCurrentContent(DependencyObject obj)
    {
        return obj.GetValue(CurrentContentProperty);
    }

    public static void SetCurrentContent(DependencyObject obj, object value)
    {
        obj.SetValue(CurrentContentProperty, value);
    }

    /// <summary>
    /// Called by RegionManager to display the resolved view in the host.
    /// </summary>
    internal static void SetHostContent(DependencyObject host, object content)
    {
        if (host is ContentControl cc)
            cc.Content = content;
        else
            SetCurrentContent(host, content);
    }

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        string name = e.NewValue as string;
        if (string.IsNullOrEmpty(name))
            return;
        try
        {
            IServiceProvider provider = RegionServiceProvider.Current;
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
}
