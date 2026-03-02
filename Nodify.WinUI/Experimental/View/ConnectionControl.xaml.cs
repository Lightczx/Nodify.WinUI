using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View;

public sealed partial class ConnectionControl : UserControl
{
    public ConnectionControl()
    {
        InitializeComponent();
    }

    public event EventHandler<ConnectionViewModel?>? ConnectionRemoved;

    public ConnectionViewModel? ViewModel
    {
        get => DataContext as ConnectionViewModel;
        set
        {
            ConnectionViewModel? @field = ViewModel;
            if (@field is not null)
            {
                @field.PropertyChanged -= OnViewModelPropertyChanged;
            }

            @field = value;
            DataContext = value;

            if (@field is not null)
            {
                @field.PropertyChanged += OnViewModelPropertyChanged;
                UpdatePath();
            }
        }
    }

    private static bool IsValidPoint(Point point)
    {
        // Check for NaN, Infinity, or invalid values
        return !double.IsNaN(point.X)
            && !double.IsNaN(point.Y)
            && !double.IsInfinity(point.X)
            && !double.IsInfinity(point.Y);
    }

    private static PathGeometry CreateBezierGeometry(Point start, Point end)
    {
        PathFigure pathFigure = new()
        {
            StartPoint = start
        };

        // Calculate control points for a smooth Bézier curve
        double distance = end.X - start.X;
        double controlPointOffset = Math.Max(Math.Abs(distance) * 0.5, 50);

        Point controlPoint1 = new(start.X + controlPointOffset, start.Y);
        Point controlPoint2 = new(end.X - controlPointOffset, end.Y);

        BezierSegment bezierSegment = new()
        {
            Point1 = controlPoint1,
            Point2 = controlPoint2,
            Point3 = end
        };

        pathFigure.Segments.Add(bezierSegment);

        PathGeometry pathGeometry = new();
        pathGeometry.Figures.Add(pathFigure);

        return pathGeometry;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName
            is nameof(ConnectionViewModel.SourcePoint)
            or nameof(ConnectionViewModel.TargetPoint)
            or nameof(ConnectionViewModel.IsSelected))
        {
            UpdatePath();
        }
    }

    private void UpdatePath()
    {
        if (ViewModel is null)
        {
            return;
        }

        // Validate points before creating geometry
        if (!IsValidPoint(ViewModel.SourcePoint) || !IsValidPoint(ViewModel.TargetPoint))
        {
            // Don't update path with invalid points
            System.Diagnostics.Debug.WriteLine($"[ConnectionControl] UpdatePath skipped - Invalid points: Source({ViewModel.SourcePoint.X}, {ViewModel.SourcePoint.Y}), Target({ViewModel.TargetPoint.X}, {ViewModel.TargetPoint.Y})");
            return;
        }

        // In WinUI 3, each Path needs its own Geometry instance
        ConnectionPath.Data = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);
        SelectionPath.Data = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);

        SelectionPath.Visibility = ViewModel.IsSelected ? Visibility.Visible : Visibility.Collapsed;
        
        System.Diagnostics.Debug.WriteLine($"[ConnectionControl] UpdatePath - Path updated from ({ViewModel.SourcePoint.X:F2}, {ViewModel.SourcePoint.Y:F2}) to ({ViewModel.TargetPoint.X:F2}, {ViewModel.TargetPoint.Y:F2})");
    }

    private void OnConnectionPathPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ConnectionPath.StrokeThickness = 4;
    }

    private void OnConnectionPathPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ConnectionPath.StrokeThickness = 3;
    }

    private void OnConnectionPathDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        // Remove connection on double-tap
        ConnectionRemoved?.Invoke(this, ViewModel);
        e.Handled = true;
    }
}