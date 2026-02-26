using System.Collections.Generic;
using LiteObservableRegions;
using LiteObservableRegions.Abstractions;

namespace LiteObservableRegions.WPF.UnitTest;

/// <summary>
/// Test view that implements <see cref="INavigationAware"/> and records every
/// <see cref="OnNavigatedFrom"/> and <see cref="OnNavigatedTo"/> call with the received <see cref="NavigationContext"/>.
/// Clear <see cref="CallLog"/> at the start of each test.
/// </summary>
public sealed class FakeNavigationAwareView : INavigationAware
{
    /// <summary>
    /// Log of callbacks: ("From"|"To", context). Clear before each test.
    /// </summary>
    public static readonly List<(string Kind, NavigationContext Context)> CallLog = new();

    public void OnNavigatedFrom(NavigationContext context) => CallLog.Add(("From", context));

    public void OnNavigatedTo(NavigationContext context) => CallLog.Add(("To", context));
}
