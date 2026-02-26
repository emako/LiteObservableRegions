using LiteObservableRegions.Abstractions;
using System.Windows;
using System.Windows.Controls;

namespace LiteObservableRegions;

/// <summary>
/// Default strategy: ContentControl uses .Content; other hosts use ObservableRegion.CurrentContent attached property.
/// </summary>
public class DefaultRegionHostContentAdapter : IRegionHostContentAdapter
{
    /// <summary>
    /// KeepAlive: High performance switching
    /// </summary>
    public virtual bool IsPreferKeepAlive { get; set; } = true;

    /// <inheritdoc />
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
