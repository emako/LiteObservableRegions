namespace LiteObservableRegions;

/// <summary>
/// Navigation mode for the current transition.
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// New navigation (push onto back stack).
    /// </summary>
    Navigate,

    /// <summary>
    /// Redirect (replace current, no back stack push).
    /// </summary>
    Redirect,

    /// <summary>
    /// Navigating back.
    /// </summary>
    GoBack,

    /// <summary>
    /// Navigating forward.
    /// </summary>
    GoForward,
}
