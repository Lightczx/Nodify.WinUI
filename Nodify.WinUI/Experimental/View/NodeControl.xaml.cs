using System;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media;

namespace Nodify.WinUI.Experimental.View;

public sealed partial class NodeControl : UserControl
{
    private bool isDragging;
    private Point dragStartPoint;
    private bool needsPortPositionUpdate;

    public NodeControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public event EventHandler<NodeViewModel>? NodeMoved;
    public event EventHandler? PortsChanged;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Update port positions after the node is loaded
        UpdatePortPositions();
    }

    public NodeViewModel? ViewModel
    {
        get => DataContext as NodeViewModel;
        set
        {
            NodeViewModel? @field = ViewModel;
            if (@field is not null)
            {
                @field.PropertyChanged -= OnViewModelPropertyChanged;
                // Unsubscribe from port collection changes
                @field.InputPorts.CollectionChanged -= OnPortsCollectionChanged;
                @field.OutputPorts.CollectionChanged -= OnPortsCollectionChanged;
            }

            @field = value;
            DataContext = value;

            if (@field is not null)
            {
                @field.PropertyChanged += OnViewModelPropertyChanged;
                Canvas.SetLeft(this, @field.X);
                Canvas.SetTop(this, @field.Y);

                // Subscribe to port collection changes
                @field.InputPorts.CollectionChanged += OnPortsCollectionChanged;
                @field.OutputPorts.CollectionChanged += OnPortsCollectionChanged;
            }
        }
    }

    private void OnPortsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // When ports are added or removed, schedule position update after layout completes
        needsPortPositionUpdate = true;
        LayoutUpdated += OnLayoutUpdatedForPortPositions;

        // Notify that ports have changed so event subscriptions can be updated
        PortsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnLayoutUpdatedForPortPositions(object? sender, object e)
    {
        if (!needsPortPositionUpdate)
        {
            return;
        }

        // Unsubscribe immediately to avoid multiple calls
        LayoutUpdated -= OnLayoutUpdatedForPortPositions;
        needsPortPositionUpdate = false;

        UpdatePortPositions();
    }

    public void UpdatePortPositions()
    {
        // Update input ports
        if (InputPortsControl.Items != null)
        {
            for (int i = 0; i < InputPortsControl.Items.Count; i++)
            {
                FrameworkElement? container = InputPortsControl.ContainerFromIndex(i) as FrameworkElement;
                if (container == null)
                {
                    continue;
                }

                PortControl? portControl = FindPortControl(container);
                if (portControl != null)
                {
                    portControl.UpdatePosition();
                }
            }
        }

        // Update output ports
        if (OutputPortsControl.Items != null)
        {
            for (int i = 0; i < OutputPortsControl.Items.Count; i++)
            {
                FrameworkElement? container = OutputPortsControl.ContainerFromIndex(i) as FrameworkElement;
                if (container == null)
                {
                    continue;
                }

                PortControl? portControl = FindPortControl(container);
                if (portControl != null)
                {
                    portControl.UpdatePosition();
                }
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (e.PropertyName is nameof(NodeViewModel.X))
        {
            Canvas.SetLeft(this, ViewModel.X);
        }
        else if (e.PropertyName is nameof(NodeViewModel.Y))
        {
            Canvas.SetTop(this, ViewModel.Y);
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Only start drag from title bar
        Point titleBarPosition = TitleBar.TransformToVisual(this).TransformPoint(default);
        Rect titleBarBounds = new (titleBarPosition.X, titleBarPosition.Y, TitleBar.ActualWidth, TitleBar.ActualHeight);

        if (!titleBarBounds.Contains(e.GetCurrentPoint(this).Position))
        {
            return;
        }

        isDragging = true;
        dragStartPoint = e.GetCurrentPoint(Parent as UIElement).Position;
        CapturePointer(e.Pointer);

        ViewModel?.IsSelected = true;
        e.Handled = true;
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!isDragging || ViewModel is null)
        {
            return;
        }

        Point currentPoint = e.GetCurrentPoint(Parent as UIElement).Position;
        double deltaX = currentPoint.X - dragStartPoint.X;
        double deltaY = currentPoint.Y - dragStartPoint.Y;

        ViewModel.X += deltaX;
        ViewModel.Y += deltaY;

        Canvas.SetLeft(this, ViewModel.X);
        Canvas.SetTop(this, ViewModel.Y);

        dragStartPoint = currentPoint;

        NodeMoved?.Invoke(this, ViewModel);
        UpdatePortPositions();

        e.Handled = true;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private static PortControl? FindPortControl(DependencyObject parent)
    {
        if (parent is PortControl portControl)
        {
            return portControl;
        }

        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject? child = VisualTreeHelper.GetChild(parent, i);
            PortControl? result = FindPortControl(child);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}