using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Nodify.WinUI.Experimental.Model;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View;

public sealed partial class PortControl : UserControl
{
    private Point lastKnownPosition = new(double.NaN, double.NaN);
    private bool isUpdatingPosition;

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
                UpdatePortPosition();
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
        UpdatePortPosition();

        // Register with NodeEditorCanvas
        NodeEditorCanvas? canvas = FindNodeEditorCanvas();
        if (canvas != null)
        {
            canvas.RegisterPortControl(this);
        }
    }

    private void OnLayoutUpdated(object? sender, object e)
    {
        // When layout changes (e.g., ports added/removed), update position
        if (!isUpdatingPosition && ViewModel != null)
        {
            UpdatePortPosition();
        }
    }

    private void UpdatePortPosition()
    {
        if (ViewModel is null)
        {
            return;
        }

        if (isUpdatingPosition)
        {
            return;
        }

        isUpdatingPosition = true;

        try
        {
            // Check if the control is loaded and has valid size
            if (ActualWidth is 0 || ActualHeight is 0)
            {
                return;
            }

            UIElement? canvas = GetCanvasParent();
            if (canvas is null)
            {
                return;
            }

            // 根据方向选取对应椭圆，计算其中心在 canvas 坐标系中的位置
            Ellipse activeEllipse = ViewModel.Direction == PortDirection.Input ? PortEllipse : PortEllipseOutput;
            Point ellipseCenter = new(activeEllipse.ActualWidth / 2, activeEllipse.ActualHeight / 2);
            Point position = activeEllipse.TransformToVisual(canvas).TransformPoint(ellipseCenter);

            // Check if position actually changed from last known position
            if (double.IsNaN(lastKnownPosition.X) ||
                Math.Abs(lastKnownPosition.X - position.X) > 0.1 ||
                Math.Abs(lastKnownPosition.Y - position.Y) > 0.1)
            {
                lastKnownPosition = position;
                // Always update ViewModel.Position to trigger PropertyChanged
                ViewModel.Position = position;
            }
        }
        catch (Exception)
        {
            // Silently handle exceptions
        }
        finally
        {
            isUpdatingPosition = false;
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

    private NodeEditorCanvas? FindNodeEditorCanvas()
    {
        DependencyObject? parent = this;
        while (parent != null)
        {
            parent = VisualTreeHelper.GetParent(parent);
            if (parent is NodeEditorCanvas canvas)
            {
                return canvas;
            }
        }

        return null;
    }

    private void OnPortEllipsePointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Ellipse ellipse)
        {
            ellipse.Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorTertiaryBrush"];
        }
    }

    private void OnPortEllipsePointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Ellipse ellipse)
        {
            ellipse.Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        }
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