using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View
{
    public sealed partial class NodeControl : UserControl
    {
        private bool _isDragging;
        private Point _dragStartPoint;

        public NodeViewModel ViewModel => DataContext as NodeViewModel;

        public event EventHandler<NodeViewModel> NodeMoved;

        public NodeControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += NodeControl_DataContextChanged;
        }

        private void NodeControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (ViewModel != null)
            {
                Canvas.SetLeft(this, ViewModel.X);
                Canvas.SetTop(this, ViewModel.Y);
            }
        }

        private void NodeControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Only start drag from title bar
            var position = e.GetCurrentPoint(this).Position;
            var titleBarBounds = new Rect(0, 0, this.ActualWidth, TitleBar.ActualHeight);

            if (titleBarBounds.Contains(position))
            {
                _isDragging = true;
                _dragStartPoint = e.GetCurrentPoint(this.Parent as UIElement).Position;
                this.CapturePointer(e.Pointer);
                
                if (ViewModel != null)
                    ViewModel.IsSelected = true;

                e.Handled = true;
            }
        }

        private void NodeControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging && ViewModel != null)
            {
                var currentPoint = e.GetCurrentPoint(this.Parent as UIElement).Position;
                var deltaX = currentPoint.X - _dragStartPoint.X;
                var deltaY = currentPoint.Y - _dragStartPoint.Y;

                ViewModel.X += deltaX;
                ViewModel.Y += deltaY;

                Canvas.SetLeft(this, ViewModel.X);
                Canvas.SetTop(this, ViewModel.Y);

                _dragStartPoint = currentPoint;

                NodeMoved?.Invoke(this, ViewModel);
                UpdatePortPositions();

                e.Handled = true;
            }
        }

        private void NodeControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleasePointerCapture(e.Pointer);
                e.Handled = true;
            }
        }

        public void UpdatePortPositions()
        {
            // Update input ports
            if (InputPortsControl.Items != null)
            {
                for (int i = 0; i < InputPortsControl.Items.Count; i++)
                {
                    var container = InputPortsControl.ContainerFromIndex(i) as FrameworkElement;
                    if (container != null)
                    {
                        var portControl = FindPortControl(container);
                        portControl?.UpdatePosition();
                    }
                }
            }

            // Update output ports
            if (OutputPortsControl.Items != null)
            {
                for (int i = 0; i < OutputPortsControl.Items.Count; i++)
                {
                    var container = OutputPortsControl.ContainerFromIndex(i) as FrameworkElement;
                    if (container != null)
                    {
                        var portControl = FindPortControl(container);
                        portControl?.UpdatePosition();
                    }
                }
            }
        }

        private PortControl FindPortControl(DependencyObject parent)
        {
            if (parent is PortControl portControl)
                return portControl;

            int childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                var result = FindPortControl(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
