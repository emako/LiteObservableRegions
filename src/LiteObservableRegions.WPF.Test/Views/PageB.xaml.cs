using LiteObservableRegions.Abstractions;
using System.Windows.Controls;

namespace LiteObservableRegions.WPF.Test.Views;

public partial class PageB : UserControl, INavigationAware
{
    public string Datetime { get; set; }

    public PageB()
    {
        Datetime = DateTime.Now.Ticks.ToString();
        DataContext = this;
        InitializeComponent();
    }

    public void OnNavigatedFrom(NavigationContext context)
    {
    }

    public void OnNavigatedTo(NavigationContext context)
    {
    }
}
