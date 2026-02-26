using System;

namespace LiteObservableRegions;

public sealed class NavigationEntry
{
    public Uri Uri { get; }
    public WeakReference ViewRef { get; }
    public NavigationContext Context { get; }

    public NavigationEntry(Uri uri, object view, NavigationContext context)
    {
        Uri = uri;
        ViewRef = new WeakReference(view);
        Context = context;
    }
}
