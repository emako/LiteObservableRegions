using LiteObservableRegions.Abstractions;
using System.Windows;
using System.Windows.Controls;

namespace LiteObservableRegions;

/// <summary>
/// Default strategy for displaying the resolved view in a region host.
/// ContentControl uses <see cref="System.Windows.Controls.ContentControl.Content"/>; Frame uses <see cref="System.Windows.Controls.Frame.Navigate(object)"/>;
/// Panel/ItemsControl add the child and optionally toggle visibility (see <see cref="IsPreferKeepAlive"/>); Decorator uses Child; other hosts use <see cref="ObservableRegion.CurrentContent"/>.
/// </summary>
public class DefaultRegionHostContentAdapter : IRegionHostContentAdapter
{
    /// <summary>
    /// When true (default), for Panel and ItemsControl the adapter keeps all children and toggles visibility (Visible for active, Collapsed for others).
    /// When false, the adapter clears children/items then adds the new view.
    /// </summary>
    public virtual bool IsPreferKeepAlive { get; set; } = true;

    /// <inheritdoc />
    /// <param name="host">The region host (element with <see cref="ObservableRegion.RegionName"/>).</param>
    /// <param name="content">The view to display (typically a UIElement).</param>
    public void SetContent(DependencyObject host, object content)
    {
        if (host is ContentControl cc)
        {
            cc.Content = content;
        }
        else if (host is Frame frame)
        {
            frame.Navigate(content);
        }
        else if (host is Panel panel)
        {
            if (content is UIElement element)
            {
                if (IsPreferKeepAlive)
                {
                    if (!panel.Children.Contains(element))
                        panel.Children.Add(element);

                    foreach (UIElement elementEach in panel.Children)
                    {
                        elementEach.Visibility =
                            elementEach == element
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                    }
                }
                else
                {
                    panel.Children.Clear();
                    panel.Children.Add(element);
                }
            }
        }
        else if (host is ItemsControl itemsControl)
        {
            if (content is UIElement element)
            {
                if (IsPreferKeepAlive)
                {
                    if (!itemsControl.Items.Contains(element))
                        itemsControl.Items.Add(element);

                    foreach (UIElement elementEach in itemsControl.Items)
                    {
                        elementEach.Visibility =
                            elementEach == element
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                    }
                }
                else
                {
                    itemsControl.Items.Clear();
                    itemsControl.Items.Add(element);
                }
            }
        }
        else if (host is Decorator decorator)
        {
            decorator.Child = content as UIElement;
        }
        else
        {
            // Fallback to attached property
            ObservableRegion.SetCurrentContent(host, content);
        }
    }
}
