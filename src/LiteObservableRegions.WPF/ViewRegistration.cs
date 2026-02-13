using System;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

/// <summary>
/// A single view registration entry (target name, view type, lifetime).
/// </summary>
public struct ViewRegistration
{
    public string TargetName { get; set; }

    public Type ViewType { get; set; }

    public ServiceLifetime Lifetime { get; set; }
}
