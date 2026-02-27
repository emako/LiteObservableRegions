using System;
using System.Collections.Generic;
using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

/// <summary>
/// Internal implementation of <see cref="IRegionViewRegistry"/>. Registers view types in the service collection and keeps a list of entries for resolution.
/// </summary>
/// <remarks>
/// Creates a registry that will add view types to the given service collection.
/// </remarks>
/// <param name="services">The service collection (must not be null).</param>
internal sealed class RegionViewRegistry(IServiceCollection services) : IRegionViewRegistry
{
    private readonly IServiceCollection _services = services ?? throw new ArgumentNullException(nameof(services));

    /// <inheritdoc />
    public IReadOnlyList<ViewRegistration> GetEntries() => _entries;

    private readonly List<ViewRegistration> _entries = [];

    /// <inheritdoc />
    /// <param name="targetName">The target name used in navigation.</param>
    /// <param name="lifetime">Transient, Scoped, or Singleton.</param>
    public void AddView<TView>(string targetName, ServiceLifetime lifetime) where TView : class
    {
        if (string.IsNullOrEmpty(targetName)) throw new ArgumentNullException(nameof(targetName));
        Type type = typeof(TView);
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                _services.AddSingleton(type);
                break;

            case ServiceLifetime.Scoped:
                _services.AddScoped(type);
                break;

            case ServiceLifetime.Transient:
                _services.AddTransient(type);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime));
        }
        _entries.Add(new ViewRegistration { TargetName = targetName, ViewType = type, Lifetime = lifetime });
    }
}
