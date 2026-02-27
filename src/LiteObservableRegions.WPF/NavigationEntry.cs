using System;

namespace LiteObservableRegions;

/// <summary>
/// A single journal entry: URI, weak reference to the view, and navigation context.
/// Used in back/forward stacks and for the current entry.
/// </summary>
/// <remarks>
/// Creates a new navigation entry.
/// </remarks>
/// <param name="uri">The region URI.</param>
/// <param name="view">The view instance (stored weakly).</param>
/// <param name="context">The navigation context.</param>
public sealed class NavigationEntry(Uri uri, object view, NavigationContext context)
{
    /// <summary>
    /// The region URI for this entry.
    /// </summary>
    public Uri Uri { get; } = uri;

    /// <summary>
    /// Weak reference to the view instance. If collected, the view can be re-resolved from DI when going back/forward.
    /// </summary>
    public WeakReference ViewRef { get; } = new WeakReference(view);

    /// <summary>
    /// The navigation context (from/to URI, parameters, mode, region name, target name).
    /// </summary>
    public NavigationContext Context { get; } = context;
}
