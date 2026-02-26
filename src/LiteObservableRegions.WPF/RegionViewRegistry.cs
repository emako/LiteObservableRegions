using System;
using System.Collections.Generic;
using LiteObservableRegions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LiteObservableRegions;

internal sealed class RegionViewRegistry : IRegionViewRegistry
{
    private readonly IServiceCollection _services;

    public IReadOnlyList<ViewRegistration> GetEntries() => _entries;

    private readonly List<ViewRegistration> _entries = [];

    public RegionViewRegistry(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

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
