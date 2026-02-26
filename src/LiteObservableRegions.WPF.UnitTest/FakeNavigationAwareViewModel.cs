using System.Collections.Generic;
using LiteObservableRegions;
using LiteObservableRegions.Abstractions;

namespace LiteObservableRegions.WPF.UnitTest;

/// <summary>
/// View-model that implements <see cref="INavigationAware"/> and records callbacks.
/// Used to test the DataContext path (view is FrameworkElement, DataContext is INavigationAware).
/// Clear <see cref="CallLog"/> before each test.
/// </summary>
public sealed class FakeNavigationAwareViewModel : INavigationAware
{
    public static readonly List<(string Kind, NavigationContext Context)> CallLog = new();

    public void OnNavigatedFrom(NavigationContext context) => CallLog.Add(("From", context));

    public void OnNavigatedTo(NavigationContext context) => CallLog.Add(("To", context));
}
