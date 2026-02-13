using LiteObservableRegions.Abstractions;
using System.Windows.Controls;

namespace LiteObservableRegions.WPF.Test.Views;

/// <summary>
/// Sample view B used for testing region navigation.
/// 测试用示例视图 B，用于验证区域导航。
/// </summary>
public partial class PageB : UserControl, INavigationAware
{
    public string Datetime { get; set; }

    public PageB()
    {
        Datetime = DateTime.Now.Ticks.ToString();
        DataContext = this;
        InitializeComponent();
    }

    /// <inheritdoc />
    public void OnNavigatedFrom(RegionNavigationEntry entry)
    {
    }

    /// <inheritdoc />
    public void OnNavigatedTo(RegionNavigationEntry entry)
    {
    }
}
