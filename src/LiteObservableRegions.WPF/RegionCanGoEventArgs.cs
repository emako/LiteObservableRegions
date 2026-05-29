using System;
using System.Diagnostics;

namespace LiteObservableRegions;

/// <summary>
/// Event arguments for a <see cref="RegionEventNotifier.ObservableRegionCanGo"/> query.
/// Subscribers can set <see cref="CanGo"/> to <c>false</c> to indicate that navigation should not proceed.
/// </summary>
/// <remarks>
/// This event is raised only when <see cref="Abstractions.IRegionNavigation.CanGo"/> is explicitly called by application code.
/// It does <b>not</b> affect Navigate, Redirect, GoBack, or GoForward — those methods always execute regardless of subscriber votes.
/// </remarks>
/// <param name="uri">The target region URI (must not be null).</param>
[DebuggerDisplay("{ToString()}")]
public sealed class RegionCanGoEventArgs(Uri uri) : EventArgs
{
    /// <summary>
    /// The target region URI being queried.
    /// </summary>
    public Uri Uri { get; } = uri ?? throw new ArgumentNullException(nameof(uri));

    /// <summary>
    /// Gets or sets whether navigation is allowed. Defaults to <c>true</c>.
    /// Any subscriber may set this to <c>false</c> to veto the navigation;
    /// once set to <c>false</c> it should not be reset to <c>true</c>.
    /// </summary>
    public bool CanGo { get; set; } = true;

    /// <inheritdoc />
    public override string ToString()
        => $"CanGo={CanGo} ({Uri?.OriginalString ?? Uri?.ToString()})";
}
