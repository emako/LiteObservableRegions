using LiteObservableRegions.Abstractions;
using System;

namespace LiteObservableRegions;

/// <summary>
/// Read-only view of a registered region (host, current URI, back/forward state).
/// Returned by <see cref="IRegionManager.GetRegion"/>.
/// </summary>
public sealed class RegionView : IRegion
{
    private readonly RegionManager _manager;
    private readonly string _name;

    /// <summary>
    /// Creates a view over the given region.
    /// </summary>
    /// <param name="manager">The region manager that owns the region.</param>
    /// <param name="name">The region name.</param>
    internal RegionView(RegionManager manager, string name)
    {
        _manager = manager;
        _name = name;
    }

    /// <inheritdoc />
    public string Name => _name;

    /// <inheritdoc />
    public object Host
    {
        get
        {
            return _manager.Regions.TryGetValue(_name, out RegionState state) ? state.Host : null;
        }
    }

    /// <inheritdoc />
    public Uri CurrentUri
    {
        get
        {
            if (!_manager.Regions.TryGetValue(_name, out RegionState state) || state.CurrentEntry == null)
                return null;
            return state.CurrentEntry.Uri;
        }
    }

    /// <inheritdoc />
    public bool CanGoBack => _manager.CanGoBack(_name);

    /// <inheritdoc />
    public bool CanGoForward => _manager.CanGoForward(_name);
}
