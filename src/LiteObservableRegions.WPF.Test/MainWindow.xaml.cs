using LiteObservableRegions.Abstractions;
using System.Diagnostics;
using System.Windows;

namespace LiteObservableRegions.WPF.Test;

public partial class MainWindow : Window
{
    private readonly IRegionManager _regionManager;

    public MainWindow()
    {
        InitializeComponent();
        _regionManager = WeakReferenceRegionHub.RegionManager;

        WeakReferenceRegionHub.ObservableRegionChanged += OnRegionChanged;

        Loaded += (s, e) =>
        {
            _regionManager?.Navigate(new Uri("region://MainGridRegion/GridA", UriKind.Absolute));
        };
    }

    private void OnRegionChanged(object? sender, RegionChangedEventArgs e)
    {
        Debug.WriteLine($"[RegionChanged] {e}");
    }

    private void BtnA_Click(object? sender, RoutedEventArgs e)
    {
        _regionManager?.Navigate(new Uri("region://MainGridRegion/GridA?a=1&b=2", UriKind.Absolute));
    }

    private void BtnB_Click(object? sender, RoutedEventArgs e)
    {
        _regionManager?.Navigate(new Uri("region://MainGridRegion/GridB?a=1&b=2", UriKind.Absolute));
    }

    private void BtnBack_Click(object? sender, RoutedEventArgs e)
    {
        _regionManager?.GoBack("MainGridRegion");
    }

    private void BtnForward_Click(object? sender, RoutedEventArgs e)
    {
        _regionManager?.GoForward("MainGridRegion");
    }

    private void BtnRedirect_Click(object? sender, RoutedEventArgs e)
    {
        _regionManager?.Redirect(new Uri("region://MainGridRegion/GridA", UriKind.Absolute));
    }

    private void BtnTest_Click(object? sender, RoutedEventArgs e)
    {
        // Open internal objects ...
        _ = _regionManager.Regions;

        Debugger.Break();
    }
}
