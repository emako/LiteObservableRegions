using LiteObservableRegions.Abstractions;
using System.Windows.Controls;

namespace LiteObservableRegions.WPF.Test.Views;

public partial class PageA : UserControl, INavigationAware
{
    public string Datetime { get; set; }

    public PageA()
    {
        Datetime = DateTime.Now.Ticks.ToString();
        DataContext = this;
        InitializeComponent();
    }

    public void OnNavigatedFrom(NavigationContext context)
    {
        // Optional: use context.Parameters, context.ToUri, etc.
    }

    public void OnNavigatedTo(NavigationContext context)
    {
        // Optional: use context.Parameters
    }
}
