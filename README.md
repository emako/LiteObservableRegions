[![NuGet](https://img.shields.io/nuget/v/LiteObservableRegions.svg)](https://nuget.org/packages/LiteObservableRegions) [![Actions](https://github.com/emako/LiteObservableRegions/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/emako/LiteObservableRegions/actions/workflows/library.nuget.yml)

# LiteObservableRegions

A lightweight, Prism-like region and navigation library for WPF. Define regions in XAML, navigate by URI, and resolve views from Microsoft.Extensions.DependencyInjection or from named host children.

---

## Features

- **XAML-first regions** — Attached properties `ObservableRegion.RegionName` and `ObservableRegion.ViewName` on any `DependencyObject`. The element with `RegionName` is the region host (content is displayed here); `ViewName` is set on a **child** of that host to register a named view.
- **URI-based navigation** — Format `region://RegionName/TargetName?query`. Navigate (push), Redirect (replace and clear back stack), GoBack, GoForward per region. Region names are compared case-insensitively.
- **Two ways to resolve views**
  - **Named views** — Set `ViewName="region://RegionName/TargetName"` on a child of the host; that element is used when navigating to that target (no DI). The host must already be registered (e.g. parent has `RegionName`); region name in `ViewName` must match the host’s region name (case-insensitive).
  - **DI views** — Register with `AddObservableRegions(reg => reg.AddView<T>("TargetName", lifetime))` or `AddRegionViews(...)`; views are resolved from the service provider (Transient, Scoped per region, Singleton). Scoped creates one scope per region; Singleton is cached per region+target.
- **Navigation stack** — Per-region back and forward stacks. Redirect clears the back stack and does not push. Navigate to the same URI as current is a no-op (no stack or content change).
- **INavigationAware** — Optional: implement on the view or on the view’s `DataContext`. `OnNavigatedFrom(NavigationContext)` and `OnNavigatedTo(NavigationContext)` receive FromUri, ToUri, Parameters, Mode, RegionName, TargetName; `context.IsRedirect` for Redirect.
- **Pluggable host adapter** — `IRegionHostContentAdapter.SetContent(host, content)`. Register before `AddObservableRegions` to customize. Default: ContentControl→Content, Frame→Navigate(content), Panel/ItemsControl→add child and toggle visibility when `IsPreferKeepAlive` is true, else clear then add; Decorator→Child; other hosts→`ObservableRegion.CurrentContent`.
- **Region context** — `ObservableRegion.RegionContext` attached property for arbitrary data; no change notifications.
- **Region change notification** — Subscribe to `WeakReferenceRegionHub.ObservableRegionChanged`; raised **before** content changes (before OnNavigatedFrom/OnNavigatedTo). Args: RegionName, FromUri, ToUri, FromTargetName, ToTargetName, `NavigationMode` (Navigate, Redirect, GoBack, GoForward), and `Cancel` to abort the navigation.
- **Lifetime** — Region host: when a `FrameworkElement` with `RegionName` is unloaded, the region is unregistered and its scope disposed. Named view: when a `FrameworkElement` with `ViewName` is unloaded, that named view is removed from the region.

---

## Installation

```bash
dotnet add package LiteObservableRegions
```

**Targets:** .NET Framework 4.6.2, 4.7.2, 4.8, 4.8.1; .NET 5–10 (Windows, WPF). Single package, multi-targeted.

---

## Quick start

### 1. Register services and set the service provider

In app startup (e.g. `App.xaml.cs`), build the DI container and set the static service provider so XAML-attached registration can resolve `IRegionManager`:

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
var provider = services.BuildServiceProvider();
WeakReferenceRegionHub.ServiceProvider = provider;
```

### 2. Define a region in XAML

The **host** is the element that receives the active view (e.g. a `ContentControl` or a `Grid`). Set `RegionName` on that element. Accepts `"region://Name"` or `"Name"` (normalized by the library).

```xml
<ContentControl xmlns:r="clr-namespace:LiteObservableRegions;assembly=LiteObservableRegions"
               r:ObservableRegion.RegionName="MainRegion" />
```

Or use a Grid as host — content is set on the Grid via `ObservableRegion.CurrentContent`; bind a child (e.g. `ContentPresenter`) to that attached property on the ancestor, or use a single ContentControl as host instead.

**Named views** (no DI): set `ViewName` on a **child** of the host. The first path segment is the target name used for navigation.

```xml
<Grid r:ObservableRegion.RegionName="region://TestGridRegion">
    <Grid r:ObservableRegion.ViewName="region://TestGridRegion/View1" />
    <Grid r:ObservableRegion.ViewName="region://TestGridRegion/View2" />
</Grid>
```

Here the host is the outer Grid; content is shown via `CurrentContent` on that Grid. For a single child host like ContentControl, put `RegionName` on the ContentControl so content goes to `Content`.

### 3. Navigate

```csharp
IRegionManager manager = WeakReferenceRegionHub.RegionManager;

manager.Navigate(new Uri("region://MainRegion/ViewA"));   // push onto back stack
manager.Redirect(new Uri("region://MainRegion/ViewB"));   // replace, clear back stack

if (manager.CanGoBack("MainRegion"))
    manager.GoBack("MainRegion");
if (manager.CanGoForward("MainRegion"))
    manager.GoForward("MainRegion");

IRegion region = manager.GetRegion("MainRegion");
Uri current = region?.CurrentUri;
object host = region?.Host;   // IRegion: Name, Host, CurrentUri, CanGoBack, CanGoForward
```

Advanced: `manager.Regions` is a `Dictionary<string, RegionState>` (Host, BackStack, ForwardStack, CurrentEntry, NamedViews, Scope).

### 4. Region change notification and cancellation

`WeakReferenceRegionHub.ObservableRegionChanged` is raised **before** the region content is updated. Set `e.Cancel = true` to prevent the navigation.

```csharp
WeakReferenceRegionHub.ObservableRegionChanged += (sender, e) =>
{
    // e.RegionName, e.FromUri, e.ToUri, e.FromTargetName, e.ToTargetName, e.Mode, e.Cancel
    if (someCondition)
        e.Cancel = true;
};
```

`e.Mode` is `NavigationMode`: `Navigate`, `Redirect`, `GoBack`, `GoForward`.

---

## Region URIs

- **Format:** `region://RegionName/TargetName?param1=value1&param2=value2`
- **Scheme:** `RegionUriParser.Scheme` (`"region"`).
- **Region name** — Authority part; must match a registered region. Preserved casing (URI host is not used for casing).
- **Target name** — First path segment; view identifier (named view or registry key).
- **Query** — Optional; parsed into `NavigationContext.Parameters` and passed to `INavigationAware`.

Helpers:

- `RegionUriParser.NormalizeRegionName(value)` — strips `region://` prefix if present.
- `RegionUriParser.TryParse(uri, out regionName, out targetName, out parameters)` — returns false if scheme or target missing.
- `RegionUriParser.ParseQuery(query)` — returns dictionary of query parameters.
- `RegionUriParser.BuildUri(regionName, targetName, parameters)` — parameters optional.

For navigation you must include a target: `region://MainRegion/View1`. Region name only (`region://MainRegion`) is valid for `RegionName` in XAML; the library normalizes it.

---

## Attached properties (`ObservableRegion`)

| Property         | Type   | Description |
|------------------|--------|-------------|
| **RegionName**   | string | On the **host** element. Registers the element as a region. Accepts `"region://Name"` or `"Name"`. |
| **ViewName**     | string | On a **child** of the host. Value like `region://RegionName/View1`; the child is registered as the view for that target (no DI). |
| **CurrentContent** | object | Set by the library; current view in the region. Can be bound. |
| **RegionContext**  | object | Arbitrary data; no change handling. |

---

## View resolution order

When navigating to a target name in a region:

1. **Named views** — If the region has a view registered for that name (via `ViewName`), that instance is used (weak reference; re-resolved from DI if collected).
2. **DI** — Otherwise resolved from `IRegionViewRegistry` and `IServiceProvider`: Transient (new each time), Scoped (per-region scope), Singleton (cached per region+target).

Register view types with `AddObservableRegions(reg => reg.AddView<T>("TargetName", ServiceLifetime.Scoped))` or, for registry only, `AddRegionViews(reg => ...)` then register `IRegionManager` yourself.

---

## Customizing how content is displayed

Implement `IRegionHostContentAdapter` (single method: `SetContent(DependencyObject host, object content)`) and register it in DI **before** `AddObservableRegions`. The default adapter:

- `ContentControl` → set `Content`
- `Frame` → `Navigate(content)`
- `Panel` / `ItemsControl` → if `IsPreferKeepAlive` is true (default): add child if not present, then set visibility (active = Visible, others = Collapsed); if false: clear then add
- `Decorator` → set `Child`
- Any other host → `ObservableRegion.SetCurrentContent(host, content)`

---

## Duplicate names and cleanup

- **Same RegionName** — Registering again replaces the previous region: existing scope is disposed, new host is the only one for that name.
- **Same ViewName** (same region) — Registering again overwrites the previous named view for that target.

**Clear:** `WeakReferenceRegionHub.Clear()` (or `IRegionManager.Clear()`) clears all named views and the singleton view cache; does **not** unregister regions or navigation stacks.

---

## API overview

- **LiteObservableRegions:** `WeakReferenceRegionHub` (ServiceProvider, RegionManager, ObservableRegionChanged, Clear), `ObservableRegion`, `RegionUriParser`, `RegionChangedEventArgs`, `NavigationMode`, `NavigationContext`, `DefaultRegionHostContentAdapter`, `RegionServiceCollectionExtensions` (AddRegionViews, AddObservableRegions), `RegionManager`, `RegionState`, `ViewRegistration`, `NavigationEntry`.
- **LiteObservableRegions.Abstractions:** `IRegionManager`, `IRegionNavigation`, `IRegion`, `IRegionViewRegistry`, `IRegionHostContentAdapter`, `INavigationAware`.

---

## License

MIT
