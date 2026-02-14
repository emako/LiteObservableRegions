using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace LiteObservableRegions.WPF.UnitTest;

public sealed class ContentAdapterTests
{
    private readonly DefaultRegionHostContentAdapter _adapter = new();

    [Fact]
    public void SetContent_Fallback_DependencyObject_SetsCurrentContentAttachedProperty()
    {
        FakeHost host = new();
        object content = new();

        _adapter.SetContent(host, content);

        Assert.Same(content, ObservableRegion.GetCurrentContent(host));
    }

    [StaFact]
    public void SetContent_ContentControl_SetsContentProperty()
    {
        ContentControl host = new();
        object content = new();

        _adapter.SetContent(host, content);

        Assert.Same(content, host.Content);
    }

    [StaFact]
    public void SetContent_Panel_IsPreferKeepAliveTrue_AddsElement_AndTogglesVisibility()
    {
        _adapter.IsPreferKeepAlive = true;
        Grid panel = new();
        TextBlock a = new() { Text = "A" };
        TextBlock b = new() { Text = "B" };

        _adapter.SetContent(panel, a);
        Assert.Single(panel.Children);
        Assert.Same(a, panel.Children[0]);
        Assert.Equal(Visibility.Visible, a.Visibility);

        _adapter.SetContent(panel, b);
        Assert.Equal(2, panel.Children.Count);
        Assert.Equal(Visibility.Collapsed, a.Visibility);
        Assert.Equal(Visibility.Visible, b.Visibility);
    }

    [StaFact]
    public void SetContent_Panel_IsPreferKeepAliveFalse_ClearsAndAdds()
    {
        _adapter.IsPreferKeepAlive = false;
        Grid panel = new();
        TextBlock a = new() { Text = "A" };
        TextBlock b = new() { Text = "B" };

        _adapter.SetContent(panel, a);
        Assert.Single(panel.Children);
        Assert.Same(a, panel.Children[0]);

        _adapter.SetContent(panel, b);
        Assert.Single(panel.Children);
        Assert.Same(b, panel.Children[0]);
    }

    [StaFact]
    public void SetContent_Panel_NonUIElementContent_DoesNotThrow()
    {
        Grid panel = new();
        object content = new();

        _adapter.SetContent(panel, content);

        Assert.Empty(panel.Children);
    }

    [StaFact]
    public void SetContent_ItemsControl_IsPreferKeepAliveTrue_AddsAndTogglesVisibility()
    {
        _adapter.IsPreferKeepAlive = true;
        ItemsControl itemsControl = new();
        TextBlock a = new() { Text = "A" };
        TextBlock b = new() { Text = "B" };

        _adapter.SetContent(itemsControl, a);
        Assert.Single(itemsControl.Items);
        Assert.Same(a, itemsControl.Items[0]);
        Assert.Equal(Visibility.Visible, a.Visibility);

        _adapter.SetContent(itemsControl, b);
        Assert.Equal(2, itemsControl.Items.Count);
        Assert.Equal(Visibility.Collapsed, a.Visibility);
        Assert.Equal(Visibility.Visible, b.Visibility);
    }

    [StaFact]
    public void SetContent_ItemsControl_IsPreferKeepAliveFalse_ClearsAndAdds()
    {
        _adapter.IsPreferKeepAlive = false;
        ItemsControl itemsControl = new();
        TextBlock a = new() { Text = "A" };
        TextBlock b = new() { Text = "B" };

        _adapter.SetContent(itemsControl, a);
        Assert.Single(itemsControl.Items);

        _adapter.SetContent(itemsControl, b);
        Assert.Single(itemsControl.Items);
        Assert.Same(b, itemsControl.Items[0]);
    }

    [StaFact]
    public void SetContent_Decorator_SetsChild()
    {
        Border decorator = new();
        TextBlock child = new() { Text = "Child" };

        _adapter.SetContent(decorator, child);

        Assert.Same(child, decorator.Child);
    }

    [StaFact]
    public void SetContent_Decorator_NonUIElement_SetsChildToNull()
    {
        Border decorator = new()
        {
            Child = new(),
        };

        _adapter.SetContent(decorator, new object());

        Assert.Null(decorator.Child);
    }
}
