using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nodify.WinUI.Experimental.Helpers;
using Nodify.WinUI.Experimental.ViewModel;
using System;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Windows.System;

namespace Nodify.WinUI.Experimental.View
{
    public sealed partial class NodeEditorCanvas : UserControl
    {
        private bool _isPanning;
        private Point _panStartPoint;
        private Point _panStartOffset;
        private PortViewModel _connectionStartPort;
        private NodeEditorViewModel _viewModel;

        public NodeEditorViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.Nodes.CollectionChanged -= Nodes_CollectionChanged;
                    _viewModel.Connections.CollectionChanged -= Connections_CollectionChanged;
                    _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }

                _viewModel = value;
                DataContext = value;

                if (_viewModel != null)
                {
                    _viewModel.Nodes.CollectionChanged += Nodes_CollectionChanged;
                    _viewModel.Connections.CollectionChanged += Connections_CollectionChanged;
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                    // Initialize existing items
                    foreach (var node in _viewModel.Nodes)
                    {
                        AddNodeControl(node);
                    }
                    foreach (var connection in _viewModel.Connections)
                    {
                        AddConnectionControl(connection);
                    }
                }
            }
        }

        public NodeEditorCanvas()
        {
            this.InitializeComponent();
            this.Loaded += NodeEditorCanvas_Loaded;
        }

        private void NodeEditorCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGridPattern();
        }

        private void DrawGridPattern()
        {
            // Simple grid pattern
            const int gridSize = 20;
            const int gridExtent = 5000;

            for (int x = -gridExtent; x <= gridExtent; x += gridSize)
            {
                var line = new Microsoft.UI.Xaml.Shapes.Line
                {
                    X1 = x,
                    Y1 = -gridExtent,
                    X2 = x,
                    Y2 = gridExtent,
                    Stroke = (Brush)Resources["GridLineBrush"],
                    StrokeThickness = 1
                };
                BackgroundCanvas.Children.Add(line);
            }

            for (int y = -gridExtent; y <= gridExtent; y += gridSize)
            {
                var line = new Microsoft.UI.Xaml.Shapes.Line
                {
                    X1 = -gridExtent,
                    Y1 = y,
                    X2 = gridExtent,
                    Y2 = y,
                    Stroke = (Brush)Resources["GridLineBrush"],
                    StrokeThickness = 1
                };
                BackgroundCanvas.Children.Add(line);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NodeEditorViewModel.ViewportOffsetX) ||
                e.PropertyName == nameof(NodeEditorViewModel.ViewportOffsetY) ||
                e.PropertyName == nameof(NodeEditorViewModel.ViewportScale))
            {
                UpdateTransform();
            }
        }

        private void UpdateTransform()
        {
            if (ViewModel == null) return;

            CanvasTransform.TranslateX = ViewModel.ViewportOffsetX;
            CanvasTransform.TranslateY = ViewModel.ViewportOffsetY;
            CanvasTransform.ScaleX = ViewModel.ViewportScale;
            CanvasTransform.ScaleY = ViewModel.ViewportScale;

            BackgroundTransform.TranslateX = ViewModel.ViewportOffsetX;
            BackgroundTransform.TranslateY = ViewModel.ViewportOffsetY;
            BackgroundTransform.ScaleX = ViewModel.ViewportScale;
            BackgroundTransform.ScaleY = ViewModel.ViewportScale;
        }

        private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (NodeViewModel node in e.NewItems)
                {
                    AddNodeControl(node);
                }
            }

            if (e.OldItems != null)
            {
                foreach (NodeViewModel node in e.OldItems)
                {
                    RemoveNodeControl(node);
                }
            }
        }

        private void Connections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ConnectionViewModel connection in e.NewItems)
                {
                    AddConnectionControl(connection);
                }
            }

            if (e.OldItems != null)
            {
                foreach (ConnectionViewModel connection in e.OldItems)
                {
                    RemoveConnectionControl(connection);
                }
            }
        }

        private void AddNodeControl(NodeViewModel node)
        {
            var nodeControl = new NodeControl { DataContext = node };
            nodeControl.NodeMoved += (s, n) => ViewModel?.UpdateConnectionPositions();
            
            // Subscribe to port events
            nodeControl.Loaded += (s, e) =>
            {
                SubscribeToPortEvents(nodeControl);
                UpdateNodePortPositions(nodeControl);
            };

            NodesCanvas.Children.Add(nodeControl);
        }

        private void RemoveNodeControl(NodeViewModel node)
        {
            var control = NodesCanvas.Children.OfType<NodeControl>()
                .FirstOrDefault(c => c.ViewModel == node);
            if (control != null)
            {
                NodesCanvas.Children.Remove(control);
            }
        }

        private void SubscribeToPortEvents(NodeControl nodeControl)
        {
            foreach (var port in FindPortControls(nodeControl))
            {
                port.ConnectionStarted += Port_ConnectionStarted;
                port.ConnectionCompleted += Port_ConnectionCompleted;
            }
        }

        private System.Collections.Generic.IEnumerable<PortControl> FindPortControls(DependencyObject parent)
        {
            var ports = new System.Collections.Generic.List<PortControl>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is PortControl portControl)
                {
                    ports.Add(portControl);
                }
                ports.AddRange(FindPortControls(child));
            }
            
            return ports;
        }

        private void UpdateNodePortPositions(NodeControl nodeControl)
        {
            nodeControl.UpdatePortPositions();
            ViewModel?.UpdateConnectionPositions();
        }

        private void AddConnectionControl(ConnectionViewModel connection)
        {
            var connectionControl = new ConnectionControl { DataContext = connection };
            connectionControl.ConnectionRemoved += (s, c) => ViewModel?.DeleteConnection(c);
            ConnectionsCanvas.Children.Add(connectionControl);
            connection.UpdatePoints();
        }

        private void RemoveConnectionControl(ConnectionViewModel connection)
        {
            var control = ConnectionsCanvas.Children.OfType<ConnectionControl>()
                .FirstOrDefault(c => c.ViewModel == connection);
            if (control != null)
            {
                ConnectionsCanvas.Children.Remove(control);
            }
        }

        private void Port_ConnectionStarted(object sender, PortViewModel port)
        {
            _connectionStartPort = port;
            ViewModel?.StartConnection(port);
            
            if (ViewModel?.PendingConnection != null)
            {
                PendingConnectionControl.DataContext = ViewModel.PendingConnection;
                PendingConnectionControl.Visibility = Visibility.Visible;
            }
        }

        private void Port_ConnectionCompleted(object sender, PortViewModel port)
        {
            if (_connectionStartPort != null && port != null)
            {
                ViewModel?.CompleteConnection(port);
            }

            _connectionStartPort = null;
            PendingConnectionControl.Visibility = Visibility.Collapsed;
            PendingConnectionControl.DataContext = null;
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);
            
            if (pointer.Properties.IsMiddleButtonPressed)
            {
                _isPanning = true;
                _panStartPoint = pointer.Position;
                _panStartOffset = new Point(ViewModel?.ViewportOffsetX ?? 0, ViewModel?.ViewportOffsetY ?? 0);
                MainCanvas.CapturePointer(e.Pointer);
                e.Handled = true;
            }
        }

        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);

            if (_isPanning && ViewModel != null)
            {
                var delta = new Point(
                    pointer.Position.X - _panStartPoint.X,
                    pointer.Position.Y - _panStartPoint.Y
                );

                ViewModel.ViewportOffsetX = _panStartOffset.X + delta.X;
                ViewModel.ViewportOffsetY = _panStartOffset.Y + delta.Y;
                e.Handled = true;
            }
            else if (ViewModel?.PendingConnection != null)
            {
                // Update pending connection
                var canvasPoint = e.GetCurrentPoint(MainCanvas).Position;
                ViewModel.UpdatePendingConnection(canvasPoint);
            }
        }

        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                MainCanvas.ReleasePointerCapture(e.Pointer);
                e.Handled = true;
            }

            if (ViewModel?.PendingConnection != null)
            {
                ViewModel.CancelConnection();
                PendingConnectionControl.Visibility = Visibility.Collapsed;
                PendingConnectionControl.DataContext = null;
            }
        }

        private void MainCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);
            
            if (pointer.Properties.IsHorizontalMouseWheel == false && ViewModel != null)
            {
                var delta = pointer.Properties.MouseWheelDelta;
                var zoomFactor = delta > 0 ? 1.1 : 0.9;

                // Get mouse position relative to canvas
                var mousePos = e.GetCurrentPoint(MainCanvas).Position;

                // Calculate new scale
                var newScale = ViewModel.ViewportScale * zoomFactor;
                newScale = Math.Max(0.1, Math.Min(5.0, newScale));

                // Adjust offset to zoom toward mouse position
                var scaleChange = newScale / ViewModel.ViewportScale;
                ViewModel.ViewportOffsetX = mousePos.X - (mousePos.X - ViewModel.ViewportOffsetX) * scaleChange;
                ViewModel.ViewportOffsetY = mousePos.Y - (mousePos.Y - ViewModel.ViewportOffsetY) * scaleChange;
                ViewModel.ViewportScale = newScale;

                e.Handled = true;
            }
        }

        private void MainCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                var position = e.GetPosition(MainCanvas);
                ViewModel.AddNode(position);
            }
        }

        private void AddNodeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.AddNode(new Point(100, 100));
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.ViewportScale *= 1.2;
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.ViewportScale /= 1.2;
        }

        private void ResetViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ViewportOffsetX = 0;
                ViewModel.ViewportOffsetY = 0;
                ViewModel.ViewportScale = 1.0;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            try
            {
                var window = GetWindow();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var file = await SerializationHelper.PickSaveFileAsync(hwnd);

                if (file != null)
                {
                    var state = ViewModel.GetEditorState();
                    await SerializationHelper.SaveToFileAsync(state, file);
                }
            }
            catch (Exception ex)
            {
                // Show error dialog
                var dialog = new ContentDialog
                {
                    Title = "Save Error",
                    Content = $"Failed to save: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            try
            {
                var window = GetWindow();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var file = await SerializationHelper.PickLoadFileAsync(hwnd);

                if (file != null)
                {
                    var state = await SerializationHelper.LoadFromFileAsync(file);
                    if (state != null)
                    {
                        ViewModel.LoadEditorState(state);
                    }
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Load Error",
                    Content = $"Failed to load: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private Window GetWindow()
        {
            // In WinUI 3, get the window from App
            var window = (Application.Current as App)?.m_window;
            return window;
        }
    }
}
