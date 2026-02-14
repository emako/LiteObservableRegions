using System.Windows;
using System.Windows.Controls;

namespace LiteObservableRegions;

/// <summary>
/// Default strategy: ContentControl uses .Content; other hosts use ObservableRegion.CurrentContent attached property.
/// </summary>
public class DefaultRegionHostContentAdapter : IRegionHostContentAdapter
{
    /// <inheritdoc />
    public void SetContent(DependencyObject host, object content)
    {
        if (host is ContentControl cc)
            cc.Content = content;
        else
            ObservableRegion.SetCurrentContent(host, content);
    }
}
