using System.Windows;

namespace LiteObservableRegions.WPF.Test;

public partial class MainWindow : Window
{
    private readonly IRegionManager _regionManager;

    public MainWindow()
    {
        InitializeComponent();
        _regionManager = RegionServiceProvider.GetRequiredService<IRegionManager>();
        Loaded += (s, e) =>
        {
            _regionManager.Navigate(new Uri("region://MainGridRegion/GridA", UriKind.Absolute));
        };
    }

    private void BtnA_Click(object sender, RoutedEventArgs e)
    {
        _regionManager?.Navigate(new Uri("region://MainGridRegion/GridA?a=1&b=2", UriKind.Absolute));
    }

    private void BtnB_Click(object sender, RoutedEventArgs e)
    {
        _regionManager?.Navigate(new Uri("region://MainGridRegion/GridB?a=1&b=2", UriKind.Absolute));
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        _regionManager?.GoBack("MainGridRegion");
    }

    private void BtnForward_Click(object sender, RoutedEventArgs e)
    {
        _regionManager?.GoForward("MainGridRegion");
    }

    private void BtnRedirect_Click(object sender, RoutedEventArgs e)
    {
        _regionManager?.Redirect(new Uri("region://MainGridRegion/GridA", UriKind.Absolute));
    }
}
