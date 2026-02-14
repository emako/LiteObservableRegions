using System.Windows;

namespace LiteObservableRegions.WPF.UnitTest;

/// <summary>
/// Minimal DependencyObject for tests; does not require STA thread.
/// </summary>
public sealed class FakeHost : DependencyObject;
