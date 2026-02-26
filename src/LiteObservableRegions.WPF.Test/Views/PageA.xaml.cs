using System.Windows.Controls;

namespace LiteObservableRegions.WPF.Test.Views;

public partial class PageA : UserControl, LiteObservableRegions.Abstractions.INavigationAware
{
    public string Datetime { get; set; }

    public PageA()
    {
        Datetime = DateTime.Now.Ticks.ToString();
        DataContext = this;
        InitializeComponent();
    }

    public void OnNavigatedFrom(LiteObservableRegions.NavigationContext context)
    {
        // Optional: use context.Parameters, context.ToUri, etc.
    }

    public void OnNavigatedTo(LiteObservableRegions.NavigationContext context)
    {
        // Optional: use context.Parameters
    }
}
