using LiteObservableRegions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteObservableRegions;

public sealed class RegionView : IRegion
{
    private readonly RegionManager _manager;
    private readonly string _name;

    internal RegionView(RegionManager manager, string name)
    {
        _manager = manager;
        _name = name;
    }

    public string Name => _name;

    public object Host
    {
        get
        {
            return _manager.Regions.TryGetValue(_name, out RegionState state) ? state.Host : null;
        }
    }

    public Uri CurrentUri
    {
        get
        {
            if (!_manager.Regions.TryGetValue(_name, out RegionState state) || state.CurrentEntry == null)
                return null;
            return state.CurrentEntry.Uri;
        }
    }

    public bool CanGoBack => _manager.CanGoBack(_name);
    public bool CanGoForward => _manager.CanGoForward(_name);
}
