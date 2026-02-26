namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Optional interface for views or view-models to receive navigation lifecycle callbacks.
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// Called when the view is about to be left (before navigating away).
    /// </summary>
    /// <param name="context">The navigation context (from/to URI, parameters, mode, region name, target name).</param>
    public void OnNavigatedFrom(NavigationContext context);

    /// <summary>
    /// Called when the view has been navigated to (after it becomes active).
    /// </summary>
    /// <param name="context">The navigation context.</param>
    public void OnNavigatedTo(NavigationContext context);
}
