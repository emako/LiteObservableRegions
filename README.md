[![NuGet](https://img.shields.io/nuget/v/LiteObservableRegions.svg)](https://nuget.org/packages/LiteObservableRegions) [![Actions](https://github.com/emako/LiteObservableRegions/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/emako/LiteObservableRegions/actions/workflows/library.nuget.yml) 

# LiteObservableRegions

A lightweight, Prism-like region and navigation library for WPF. Define regions in XAML, navigate by URI, and resolve views from Microsoft.Extensions.DependencyInjection or from named host children.

## Features

- **XAML-first regions** — Attached properties `RegionName` and `ViewName` on any `DependencyObject` (e.g. `Grid`, `ContentControl`).
- **URI-based navigation** — Format `region://RegionName/TargetName?query`. Navigate, redirect, go back/forward per region.
- **Two ways to resolve views**  
  - **Named views** — Children with `ViewName="region://RegionName/View1"` are registered by name; navigation to that target uses the element directly (no DI).  
  - **DI views** — Register view types with `AddRegionViews`; views are resolved from the service provider (Transient, Scoped, Singleton).
- **Navigation stack** — Per-region back/forward history; redirect clears the back stack.
- **INavigationAware** — Optional callbacks `OnNavigatedFrom` / `OnNavigatedTo` on views or view-models.
- **Pluggable host adapter** — `IRegionHostContentAdapter` controls how content is shown (default supports ContentControl, Frame, Panel, ItemsControl, Decorator).
- **Region context** — `RegionContext` attached property for arbitrary data storage (no change handling).
- **Region change notification** — Subscribe to `WeakReferenceRegionHub.ObservableRegionChanged` before any content switch; receive detailed context (region name, from/to URI and target names, Navigate/Redirect/GoBack/GoForward) and optionally cancel by setting `RegionChangedEventArgs.Cancel`.

## Installation

```bash
dotnet add package LiteObservableRegions
```

Targets: .NET Framework 4.6.2+, .NET 5–10 (Windows, WPF).

## Quick start

### 1. Register services and set the service provider

In your app startup (e.g. `App.xaml.cs`), build the DI container and set the static service provider so XAML-attached registration can resolve `IRegionManager`:

```csharp
using LiteObservableRegions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddObservableRegions(reg =>
{
    reg.AddView<PageA>("ViewA", ServiceLifetime.Scoped);
    reg.AddView<PageB>("ViewB", ServiceLifetime.Scoped);
});
// Optional: services.AddSingleton<IRegionHostContentAdapter, MyAdapter>();
IServiceProvider provider = services.BuildServiceProvider();
WeakReferenceRegionHub.ServiceProvider = provider;
```

### 2. Define a region and optional named views in XAML

```xml
<Grid xmlns:r="clr-namespace:LiteObservableRegions;assembly=LiteObservableRegions"
      r:ObservableRegion.RegionName="region://MainRegion">
    <!-- ContentControl-style host: resolved view is set as content -->
    <ContentControl x:Name="Host" />
</Grid>
```

Or use inline named views (no DI needed for these targets):

```xml
<Grid r:ObservableRegion.RegionName="region://TestGridRegion">
    <Grid r:ObservableRegion.ViewName="region://TestGridRegion/View1" />
    <Grid r:ObservableRegion.ViewName="region://TestGridRegion/View2" />
</Grid>
```

The first path segment after the region name (e.g. `View1`, `View2`) is the target name used when navigating.

### 3. Navigate

```csharp
IRegionManager manager = WeakReferenceRegionHub.RegionManager;

// Navigate (push onto back stack)
manager.Navigate(new Uri("region://MainRegion/ViewA"));

// Redirect (replace current, clear back stack)
manager.Redirect(new Uri("region://MainRegion/ViewB"));

// Back/forward
if (manager.CanGoBack("MainRegion"))
    manager.GoBack("MainRegion");
if (manager.CanGoForward("MainRegion"))
    manager.GoForward("MainRegion");

// Get region info
IRegion region = manager.GetRegion("MainRegion");
Uri current = region?.CurrentUri;
object host = region?.Host;
```

### 4. Region change notification and cancellation

Subscribe to `WeakReferenceRegionHub.ObservableRegionChanged` to be notified before a region’s content changes (Navigate, Redirect, GoBack, or GoForward). The event args provide full context and allow cancelling the change:

```csharp
WeakReferenceRegionHub.ObservableRegionChanged += (sender, e) =>
{
    // e.RegionName, e.FromUri, e.ToUri, e.FromTargetName, e.ToTargetName, e.Mode (Push/Replace/Back/Forward)
    System.Diagnostics.Debug.WriteLine(
        $"{e.Mode} in {e.RegionName}: {e.FromTargetName} -> {e.ToTargetName} ({e.ToUri})");

    // Cancel this navigation (no stack or content change will occur)
    if (someCondition)
        e.Cancel = true;
};
```

- **When:** Fired before the region content is updated (before views receive `OnNavigatedFrom`/`OnNavigatedTo`).
- **Cancel:** Set `e.Cancel = true` to prevent the navigation; the back/forward stacks and current content stay unchanged.
- **Mode:** `NavigationMode.Push` (Navigate), `Replace` (Redirect), `Back`, or `Forward`.

## Region URIs

- **Format:** `region://RegionName/TargetName?param1=value1&param2=value2`
- **Region name** — Host part; must match a registered region (e.g. from `RegionName`).
- **Target name** — First path segment; identifies the view (either a named view under that host or a key in the view registry).
- **Query** — Optional; parsed and passed in `NavigationContext.Parameters` and to `INavigationAware`.

You can use the shorthand `region://MainRegion` for the region name only (e.g. in `RegionName`); the library normalizes it. For navigation you must include a target: `region://MainRegion/View1`.

Helper: `RegionUriParser.BuildUri(regionName, targetName, parameters)`.

## Attached properties (`ObservableRegion`)

| Property         | Type   | Description |
|------------------|--------|-------------|
| **RegionName**   | string | Set on the host element. Registers the element as a region. Accepts `"region://Name"` or `"Name"`. |
| **ViewName**     | string | Set on a child of the host. Value like `region://RegionName/View1`; the child is registered as the view for `View1` so navigation to that target uses it without DI. |
| **CurrentContent** | object | Set by the library; current view displayed in the region. Can be bound. |
| **RegionContext**  | object | Storage for arbitrary data; no change handling. |

## View resolution order

When navigating to a target name in a region:

1. **Named views** — If the region has a view registered for that name (e.g. via `ViewName`), that instance is used.
2. **DI** — Otherwise the view is resolved from `IRegionViewRegistry` + `IServiceProvider` (Transient, Scoped per region, or Singleton).

Register view types with `AddRegionViews(reg => reg.AddView<MyView>("TargetName", ServiceLifetime.Scoped))`.

## Customizing how content is displayed

Implement `IRegionHostContentAdapter` and register it in DI before `AddObservableRegions`. The default adapter supports:

- `ContentControl` → `Content`
- `Frame` → `Navigate(content)`
- `Panel` / `ItemsControl` → add child and optionally toggle visibility (configurable `IsPreferKeepAlive`)
- `Decorator` → `Child`

## Duplicate names

- **Same RegionName** — Registering again replaces the previous region: existing scope is disposed, and the new host becomes the only one for that name.
- **Same ViewName** (same region) — Registering again overwrites the previous named view for that target.

## License

MIT
