using LiteObservableRegions.Abstractions;
using System.Windows.Controls;

namespace LiteObservableRegions.WPF.Test.Views;

/// <summary>
/// Sample view A used for testing region navigation.
/// 测试用示例视图 A，用于验证区域导航。
/// </summary>
public partial class PageA : UserControl, INavigationAware
{
    public string Datetime { get; set; }

    public PageA()
    {
        Datetime = DateTime.Now.Ticks.ToString();
        DataContext = this;
        InitializeComponent();
    }

    /// <inheritdoc />
    public void OnNavigatedFrom(RegionNavigationEntry entry)
    {
        Dictionary<string, object> value = RegionManager.GetRegionParameter(this);
    }

    /// <inheritdoc />
    public void OnNavigatedTo(RegionNavigationEntry entry)
    {
        Dictionary<string, object> value = RegionManager.GetRegionParameter(this);
    }
}
