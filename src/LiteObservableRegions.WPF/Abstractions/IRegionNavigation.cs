using System;

namespace LiteObservableRegions.Abstractions;

/// <summary>
/// Navigation operations for region-based navigation (history and redirect).
/// </summary>
public interface IRegionNavigation
{
    /// <summary>
    /// Navigate to the given region URI (push onto back stack).
    /// </summary>
    /// <param name="uri">Full region URI, e.g. region://MainRegion/ViewA.</param>
    public void Navigate(Uri uri);

    /// <summary>
    /// Redirect to the given region URI (replace current, clear back stack).
    /// </summary>
    /// <param name="uri">Full region URI.</param>
    public void Redirect(Uri uri);

    /// <summary>
    /// Go back in the specified region.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    public void GoBack(string regionName);

    /// <summary>
    /// Go forward in the specified region.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    public void GoForward(string regionName);

    /// <summary>
    /// Whether the region can go back.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>True if the region has at least one entry on the back stack.</returns>
    public bool CanGoBack(string regionName);

    /// <summary>
    /// Whether the region can go forward.
    /// </summary>
    /// <param name="regionName">The region name (case-insensitive).</param>
    /// <returns>True if the region has at least one entry on the forward stack.</returns>
    public bool CanGoForward(string regionName);

    /// <summary>
    /// Queries whether navigation to the given URI is currently permitted by raising
    /// <see cref="RegionEventNotifier.ObservableRegionCanGo"/>. Returns <c>false</c> if any subscriber
    /// sets <see cref="RegionCanGoEventArgs.CanGo"/> to <c>false</c>; otherwise <c>true</c>.
    /// </summary>
    /// <remarks>
    /// This method is a pure query and does <b>not</b> affect actual navigation.
    /// Navigate, Redirect, GoBack, and GoForward always execute whether or not you call this first.
    /// Call it when you want to give subscribers (e.g. unsaved-changes guards) a chance to veto
    /// before you decide to navigate.
    /// </remarks>
    /// <param name="uri">The target region URI to check (e.g. <c>region://MainRegion/ViewA</c>).</param>
    /// <returns><c>true</c> if no subscriber vetoed the navigation; <c>false</c> if at least one did.</returns>
    public bool CanGo(Uri uri);
}
