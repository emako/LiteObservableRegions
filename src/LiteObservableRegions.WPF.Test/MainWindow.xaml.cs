using Regions.Abstractions;
using System.Windows;

namespace LiteObservableRegions.WPF.Test;

/// <summary>
/// Main window used to drive region navigation for testing.
/// 主窗口：用于进行区域导航的测试。
/// </summary>
public partial class MainWindow : Window
{
    private readonly IRegionManager _regionManager;

    public MainWindow()
    {
        InitializeComponent();
        _regionManager = RegionServiceProvider.GetRequiredService<IRegionManager>();
        //_regionManager.RequestNavigate("MainRegion", new Uri("/PageA", UriKind.Relative));
        _regionManager.RequestNavigate("MainGridRegion", new Uri("/GridA", UriKind.Relative));
    }

    /// <summary>
    /// Navigate to A in the Grid region with parameters.
    /// 在 Grid 区域导航到 A，并附带参数。
    /// </summary>
    private void BtnA_Click(object sender, RoutedEventArgs e)
    {
        //_regionManager?.RequestNavigate("MainRegion", new Uri("/PageA", UriKind.Relative));
        _regionManager?.RequestNavigate("MainGridRegion", new Uri("/GridA?a=1&b=2", UriKind.Relative));
    }

    /// <summary>
    /// Navigate to B in the Grid region with parameters.
    /// 在 Grid 区域导航到 B，并附带参数。
    /// </summary>
    private void BtnB_Click(object sender, RoutedEventArgs e)
    {
        //_regionManager?.RequestNavigate("MainRegion", new Uri("/PageB", UriKind.Relative));
        _regionManager?.RequestNavigate("MainGridRegion", new Uri("/GridB?a=1&b=2", UriKind.Relative));
    }

    /// <summary>
    /// Go back using the region's journal.
    /// 使用区域导航日志执行后退。
    /// </summary>
    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        //_regionManager?.RequestGoBack("MainRegion");
        _regionManager?.RequestGoBack("MainGridRegion");
    }

    /// <summary>
    /// Go forward using the region's journal.
    /// 使用区域导航日志执行前进。
    /// </summary>
    private void BtnForward_Click(object sender, RoutedEventArgs e)
    {
        //_regionManager?.RequestGoForward("MainRegion");
        _regionManager?.RequestGoForward("MainGridRegion");
    }

    /// <summary>
    /// Clear history and navigate to initial view.
    /// 清空历史并导航到初始视图。
    /// </summary>
    private void BtnRedirect_Click(object sender, RoutedEventArgs e)
    {
        //_regionManager?.RequestRedirect("MainRegion", new Uri("/PageA", UriKind.Relative));
        _regionManager?.RequestRedirect("MainGridRegion", new Uri("/GridA", UriKind.Relative));
    }
}
