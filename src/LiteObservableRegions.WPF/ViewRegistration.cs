using System;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

/// <summary>
/// A single view registration entry (target name, view type, lifetime).
/// Returned by <see cref="LiteObservableRegions.Abstractions.IRegionViewRegistry.GetEntries"/>.
/// </summary>
public struct ViewRegistration
{
    /// <summary>
    /// The target name used in navigation (e.g. "ViewA").
    /// </summary>
    public string TargetName { get; set; }

    /// <summary>
    /// The view type registered in DI.
    /// </summary>
    public Type ViewType { get; set; }

    /// <summary>
    /// The service lifetime (Transient, Scoped, or Singleton).
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }
}
