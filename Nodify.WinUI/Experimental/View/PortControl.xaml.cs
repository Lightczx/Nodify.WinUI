using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View;

public sealed partial class PortControl : UserControl
{
    public PortControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    public event EventHandler<PortViewModel?>? ConnectionStarted;
    public event EventHandler<PortViewModel?>? ConnectionCompleted;

    public PortViewModel? ViewModel { get => DataContext as PortViewModel; }

    public void UpdatePosition()
    {
        UpdatePortPosition();
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        UpdatePortPosition();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdatePortPosition();
    }

    private void UpdatePortPosition()
    {
        if (ViewModel is null)
        {
            return;
        }

        try
        {
            // Get the center position of the port in canvas coordinates
            Point position = TransformToVisual(GetCanvasParent()).TransformPoint(new(8, 8)); // Center of the ellipse
            ViewModel.Position = position;
        }
        catch
        {
            // Ignore if transform fails
        }
    }

    private UIElement? GetCanvasParent()
    {
        DependencyObject parent = this;
        while (parent != null)
        {
            parent = VisualTreeHelper.GetParent(parent);
            if (parent is Canvas or NodeEditorCanvas)
            {
                return parent as UIElement;
            }
        }

        return null;
    }

    private void OnPortEllipsePointerEntered(object sender, PointerRoutedEventArgs e)
    {
        PortEllipse.Width = 20;
        PortEllipse.Height = 20;
    }

    private void OnPortEllipsePointerExited(object sender, PointerRoutedEventArgs e)
    {
        PortEllipse.Width = 16;
        PortEllipse.Height = 16;
    }

    private void OnPortEllipsePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        UpdatePortPosition();
        ConnectionStarted?.Invoke(this, ViewModel);
        e.Handled = true;
    }

    private void OnPortEllipsePointerReleased(object sender, PointerRoutedEventArgs e)
    {
        UpdatePortPosition();
        ConnectionCompleted?.Invoke(this, ViewModel);
        e.Handled = true;
    }
}