using System.Windows.Controls;

namespace LiteObservableRegions.WPF.UnitTest;

/// <summary>
/// View that does NOT implement INavigationAware; its DataContext does.
/// Used to test that RegionManager invokes OnNavigatedFrom/OnNavigatedTo on DataContext.
/// </summary>
public sealed class ViewWithAwareDataContext : UserControl
{
    public ViewWithAwareDataContext()
    {
        DataContext = new FakeNavigationAwareViewModel();
    }
}
