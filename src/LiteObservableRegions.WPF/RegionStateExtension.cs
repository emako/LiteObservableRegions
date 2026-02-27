namespace LiteObservableRegions;

public static class RegionStateExtension
{
    /// <summary>
    /// Disposes the scoped service scope and sets <see cref="RegionState.Scope"/> to null.
    /// Called when the region is replaced or the host is unloaded.
    /// </summary>
    public static void DisposeScope(this RegionState regionState)
    {
        regionState.Scope?.Dispose();
        regionState.Scope = null;
    }
}
