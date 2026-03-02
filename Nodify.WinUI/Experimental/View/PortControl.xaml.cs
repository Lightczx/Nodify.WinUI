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
    private Point _lastKnownPosition = new Point(double.NaN, double.NaN);
    private bool _isUpdatingPosition = false;

    public PortControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        LayoutUpdated += OnLayoutUpdated;
    }

    public event EventHandler<PortViewModel?>? ConnectionStarted;
    public event EventHandler<PortViewModel?>? ConnectionCompleted;

    public PortViewModel? ViewModel
    {
        get => DataContext as PortViewModel;
        set
        {
            PortViewModel? @field = ViewModel;
            if (@field is not null)
            {
                // No events to unsubscribe for PortViewModel currently
            }

            @field = value;
            DataContext = value;

            if (@field is not null)
            {
                // Schedule position update after layout
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    UpdatePortPosition();
                });
            }
        }
    }

    public void UpdatePosition()
    {
        UpdatePortPosition();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Update position when loaded
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            UpdatePortPosition();
        });
    }

    private void OnLayoutUpdated(object? sender, object e)
    {
        // When layout changes (e.g., ports added/removed), update position
        if (!_isUpdatingPosition && ViewModel != null)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                UpdatePortPosition();
            });
        }
    }

    private void UpdatePortPosition()
    {
        if (ViewModel is null)
        {
            System.Diagnostics.Debug.WriteLine($"[PortControl] ViewModel is null");
            return;
        }

        if (_isUpdatingPosition)
        {
            return;
        }

        _isUpdatingPosition = true;

        try
        {
            // Check if the control is loaded and has valid size
            if (ActualWidth == 0 || ActualHeight == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PortControl] Control not yet laid out for port {ViewModel.Name}");
                return;
            }

            UIElement? canvas = GetCanvasParent();
            if (canvas is null)
            {
                System.Diagnostics.Debug.WriteLine($"[PortControl] Canvas not found for port {ViewModel.Name}");
                return;
            }

            // Get the center position of the port in canvas coordinates
            Point position = TransformToVisual(canvas).TransformPoint(new(8, 8)); // Center of the ellipse
            
            // Check if position actually changed from last known position
            if (double.IsNaN(_lastKnownPosition.X) || 
                Math.Abs(_lastKnownPosition.X - position.X) > 0.1 || 
                Math.Abs(_lastKnownPosition.Y - position.Y) > 0.1)
            {
                _lastKnownPosition = position;
                // Always update ViewModel.Position to trigger PropertyChanged
                ViewModel.Position = position;
                System.Diagnostics.Debug.WriteLine($"[PortControl] Updated position for {ViewModel.Name}: ({position.X:F2}, {position.Y:F2})");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PortControl] Failed to update position: {ex.Message}");
        }
        finally
        {
            _isUpdatingPosition = false;
        }
    }

    private UIElement? GetCanvasParent()
    {
        DependencyObject? parent = this;
        while (parent != null)
        {
            parent = VisualTreeHelper.GetParent(parent);
            if (parent is Canvas canvas && canvas.Name == "NodesCanvas")
            {
                return canvas;
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