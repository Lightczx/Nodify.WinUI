using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View
{
    public sealed partial class ConnectionControl : UserControl
    {
        public ConnectionViewModel ViewModel => DataContext as ConnectionViewModel;

        public event EventHandler<ConnectionViewModel> ConnectionRemoved;

        public ConnectionControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += ConnectionControl_DataContextChanged;
        }

        private void ConnectionControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdatePath();
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectionViewModel.SourcePoint) ||
                e.PropertyName == nameof(ConnectionViewModel.TargetPoint) ||
                e.PropertyName == nameof(ConnectionViewModel.IsSelected))
            {
                UpdatePath();
            }
        }

        private void UpdatePath()
        {
            if (ViewModel == null) return;

            // Validate points before creating geometry
            if (!IsValidPoint(ViewModel.SourcePoint) || !IsValidPoint(ViewModel.TargetPoint))
            {
                // Don't update path with invalid points
                return;
            }

            // In WinUI 3, each Path needs its own Geometry instance
            ConnectionPath.Data = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);
            SelectionPath.Data = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);

            SelectionPath.Visibility = ViewModel.IsSelected ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool IsValidPoint(Point point)
        {
            // Check for NaN, Infinity, or invalid values
            return !double.IsNaN(point.X) && !double.IsNaN(point.Y) &&
                   !double.IsInfinity(point.X) && !double.IsInfinity(point.Y);
        }

        private Geometry CreateBezierGeometry(Point start, Point end)
        {
            var pathFigure = new PathFigure { StartPoint = start };

            // Calculate control points for a smooth bezier curve
            var distance = end.X - start.X;
            var controlPointOffset = Math.Max(Math.Abs(distance) * 0.5, 50);

            var controlPoint1 = new Point(start.X + controlPointOffset, start.Y);
            var controlPoint2 = new Point(end.X - controlPointOffset, end.Y);

            var bezierSegment = new BezierSegment
            {
                Point1 = controlPoint1,
                Point2 = controlPoint2,
                Point3 = end
            };

            pathFigure.Segments.Add(bezierSegment);

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private void ConnectionPath_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ConnectionPath.StrokeThickness = 4;
        }

        private void ConnectionPath_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ConnectionPath.StrokeThickness = 3;
        }

        private void ConnectionPath_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            // Remove connection on double-tap
            ConnectionRemoved?.Invoke(this, ViewModel);
            e.Handled = true;
        }
    }
}
