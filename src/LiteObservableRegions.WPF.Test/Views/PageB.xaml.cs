using System.Windows.Controls;

namespace LiteObservableRegions.WPF.Test.Views;

public partial class PageB : UserControl, LiteObservableRegions.INavigationAware
{
    public string Datetime { get; set; }

    public PageB()
    {
        Datetime = DateTime.Now.Ticks.ToString();
        DataContext = this;
        InitializeComponent();
    }

    public void OnNavigatedFrom(LiteObservableRegions.NavigationContext context)
    {
    }

    public void OnNavigatedTo(LiteObservableRegions.NavigationContext context)
    {
    }
}
