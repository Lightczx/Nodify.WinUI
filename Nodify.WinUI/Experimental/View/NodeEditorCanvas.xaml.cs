using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nodify.WinUI.Experimental.Helpers;
using Nodify.WinUI.Experimental.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Nodify.WinUI.Experimental.Model;
using WinRT;

namespace Nodify.WinUI.Experimental.View;

public sealed partial class NodeEditorCanvas : UserControl
{
    private bool isPanning;
    private Point panStartPoint;
    private Point panStartOffset;
    private PortViewModel? connectionStartPort;

    public NodeEditorViewModel? ViewModel
    {
        get;
        set
        {
            if (field is not null)
            {
                field.Nodes.CollectionChanged -= OnNodesCollectionChanged;
                field.Connections.CollectionChanged -= OnConnectionsCollectionChanged;
                field.PropertyChanged -= OnViewModelPropertyChanged;
            }

            field = value;
            DataContext = value;

            if (field is not null)
            {
                field.Nodes.CollectionChanged += OnNodesCollectionChanged;
                field.Connections.CollectionChanged += OnConnectionsCollectionChanged;
                field.PropertyChanged += OnViewModelPropertyChanged;

                // Set ViewModel for MiniMap
                MiniMap.ViewModel = field;

                // Initialize existing items
                foreach (NodeViewModel node in field.Nodes)
                {
                    AddNodeControl(node);
                }

                foreach (ConnectionViewModel connection in field.Connections)
                {
                    AddConnectionControl(connection);
                }
            }
            else
            {
                // Clear MiniMap ViewModel when null
                MiniMap.ViewModel = null;
            }
        }
    }

    public NodeEditorCanvas()
    {
        InitializeComponent();
    }

    private static List<PortControl> FindPortControls(DependencyObject parent)
    {
        List<PortControl> ports = [];
        int childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            DependencyObject? child = VisualTreeHelper.GetChild(parent, i);
            if (child is PortControl portControl)
            {
                ports.Add(portControl);
            }

            ports.AddRange(FindPortControls(child));
        }

        return ports;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName
            is nameof(NodeEditorViewModel.ViewportOffsetX)
            or nameof(NodeEditorViewModel.ViewportOffsetY)
            or nameof(NodeEditorViewModel.ViewportScale))
        {
            UpdateTransform();
        }
    }

    private void UpdateTransform()
    {
        if (ViewModel is null)
        {
            return;
        }

        CanvasTransform.TranslateX = ViewModel.ViewportOffsetX;
        CanvasTransform.TranslateY = ViewModel.ViewportOffsetY;
        CanvasTransform.ScaleX = ViewModel.ViewportScale;
        CanvasTransform.ScaleY = ViewModel.ViewportScale;

        BackgroundTransform.TranslateX = ViewModel.ViewportOffsetX;
        BackgroundTransform.TranslateY = ViewModel.ViewportOffsetY;
        BackgroundTransform.ScaleX = ViewModel.ViewportScale;
        BackgroundTransform.ScaleY = ViewModel.ViewportScale;
    }

    private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Reset)
        {
            // Clear all UI controls when collection is reset
            NodesCanvas.Children.Clear();

            // Add new items if any
            if (ViewModel is not null)
            {
                foreach (NodeViewModel node in ViewModel.Nodes)
                {
                    AddNodeControl(node);
                }
            }
        }
        else
        {
            if (e.NewItems is not null)
            {
                foreach (NodeViewModel node in e.NewItems)
                {
                    AddNodeControl(node);
                }
            }

            if (e.OldItems is not null)
            {
                foreach (NodeViewModel node in e.OldItems)
                {
                    RemoveNodeControl(node);
                }
            }
        }
    }

    private void OnConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Reset)
        {
            // Clear all UI controls when collection is reset
            ConnectionsCanvas.Children.Clear();

            // Add new items if any
            if (ViewModel is not null)
            {
                foreach (ConnectionViewModel connection in ViewModel.Connections)
                {
                    AddConnectionControl(connection);
                }
            }
        }
        else
        {
            if (e.NewItems is not null)
            {
                foreach (ConnectionViewModel connection in e.NewItems)
                {
                    AddConnectionControl(connection);
                }
            }

            if (e.OldItems is not null)
            {
                foreach (ConnectionViewModel connection in e.OldItems)
                {
                    RemoveConnectionControl(connection);
                }
            }
        }
    }

    private void AddNodeControl(NodeViewModel node)
    {
        NodeControl nodeControl = new() { DataContext = node };
        nodeControl.NodeMoved += OnNodeControlNodeMoved;
        nodeControl.PortsChanged += OnNodeControlPortsChanged;

        // Subscribe to selection changes
        node.PropertyChanged += OnNodeViewModelPropertyChanged;

        // Subscribe to port events
        nodeControl.Loaded += OnNodeControlLoaded;

        // Set initial position immediately
        Canvas.SetLeft(nodeControl, node.X);
        Canvas.SetTop(nodeControl, node.Y);

        NodesCanvas.Children.Add(nodeControl);
    }

    private void OnNodeControlNodeMoved(object? sender, NodeViewModel node)
    {
        ViewModel?.UpdateConnectionPositions();
    }

    private void OnNodeControlLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not NodeControl nodeControl)
        {
            return;
        }

        SubscribeToPortEvents(nodeControl);
        UpdateNodePortPositions(nodeControl);
    }

    private void OnNodeControlPortsChanged(object? sender, EventArgs e)
    {
        if (sender is not NodeControl nodeControl)
        {
            return;
        }

        SubscribeToPortEvents(nodeControl);
    }

    private void OnNodeViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NodeViewModel node)
        {
            return;
        }

        if (e.PropertyName is nameof(NodeViewModel.IsSelected) && node.IsSelected && ViewModel?.SelectedNode != node)
        {
            ViewModel?.SelectedNode = node;
        }
    }

    private void RemoveNodeControl(NodeViewModel? node)
    {
        if (node is null)
        {
            return;
        }

        // Try to find by ViewModel reference first
        // If not found, try to find by ID (in case DataContext was cleared)
        NodeControl? control =
            NodesCanvas.Children.OfType<NodeControl>().FirstOrDefault(c => c.ViewModel == node) ??
            NodesCanvas.Children.OfType<NodeControl>().FirstOrDefault(c => c.ViewModel?.Id == node.Id);

        if (control != null)
        {
            NodesCanvas.Children.Remove(control);
        }
    }

    private void SubscribeToPortEvents(NodeControl nodeControl)
    {
        List<PortControl> ports = FindPortControls(nodeControl);

        foreach (PortControl port in ports)
        {
            // Unsubscribe first to avoid duplicate subscriptions
            port.ConnectionStarted -= OnPortConnectionStarted;
            port.ConnectionCompleted -= OnPortConnectionCompleted;

            // Subscribe to events
            port.ConnectionStarted += OnPortConnectionStarted;
            port.ConnectionCompleted += OnPortConnectionCompleted;
        }
    }

    public void RegisterPortControl(PortControl portControl)
    {
        // Unsubscribe first to avoid duplicate subscriptions
        portControl.ConnectionStarted -= OnPortConnectionStarted;
        portControl.ConnectionCompleted -= OnPortConnectionCompleted;

        // Subscribe to events
        portControl.ConnectionStarted += OnPortConnectionStarted;
        portControl.ConnectionCompleted += OnPortConnectionCompleted;
    }

    private void UpdateNodePortPositions(NodeControl nodeControl)
    {
        nodeControl.UpdatePortPositions();
        ViewModel?.UpdateConnectionPositions();
    }

    private void UpdateAllPortPositions()
    {
        foreach (NodeControl nodeControl in NodesCanvas.Children.OfType<NodeControl>())
        {
            nodeControl.UpdatePortPositions();
        }
    }

    private async void AddConnectionControl(ConnectionViewModel connection)
    {
        ConnectionControl connectionControl = new() { ViewModel = connection };
        connectionControl.ConnectionRemoved += OnConnectionControlConnectionRemoved;
        ConnectionsCanvas.Children.Add(connectionControl);

        // Wait for layout to complete before updating connection points
        await System.Threading.Tasks.Task.Delay(50);

        // Force update port positions
        UpdateAllPortPositions();

        // Now update the connection points
        connection.UpdatePoints();
    }

    private void OnConnectionControlConnectionRemoved(object? sender, ConnectionViewModel? connection)
    {
        ViewModel?.DeleteConnection(connection);
    }

    private void RemoveConnectionControl(ConnectionViewModel? connection)
    {
        if (connection is null)
        {
            return;
        }

        // Try to find by ViewModel reference first
        // If not found, try to find by ID (in case DataContext was cleared)
        ConnectionControl? control =
            ConnectionsCanvas.Children.OfType<ConnectionControl>().FirstOrDefault(c => c.ViewModel == connection) ??
            ConnectionsCanvas.Children.OfType<ConnectionControl>().FirstOrDefault(c => c.ViewModel?.Id == connection.Id);

        if (control != null)
        {
            ConnectionsCanvas.Children.Remove(control);
        }
    }

    private void OnPortConnectionStarted(object? sender, PortViewModel? port)
    {
        connectionStartPort = port;
        ViewModel?.StartConnection(port);

        if (ViewModel?.PendingConnection == null)
        {
            return;
        }

        PendingConnectionControl.ViewModel = ViewModel.PendingConnection;
        PendingConnectionControl.Visibility = Visibility.Visible;
    }

    private async void OnPortConnectionCompleted(object? sender, PortViewModel? port)
    {
        if (connectionStartPort is not null && port is not null)
        {
            // Force update port positions before completing connection
            UpdateAllPortPositions();

            // Small delay to ensure positions are updated
            await System.Threading.Tasks.Task.Delay(10);

            ViewModel?.CompleteConnection(port);
        }

        connectionStartPort = null;
        PendingConnectionControl.Visibility = Visibility.Collapsed;
        PendingConnectionControl.ViewModel = null;
    }

    private void OnMainCanvasPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        PointerPoint? pointer = e.GetCurrentPoint(this);

        if (pointer.Properties.IsMiddleButtonPressed)
        {
            isPanning = true;
            panStartPoint = pointer.Position;
            panStartOffset = new(ViewModel?.ViewportOffsetX ?? 0, ViewModel?.ViewportOffsetY ?? 0);
            MainCanvas.CapturePointer(e.Pointer);
            e.Handled = true;
        }
        else if (pointer.Properties.IsLeftButtonPressed)
        {
            // Check if clicked on canvas background (not on a node or connection)
            if (e.OriginalSource.Equals(MainCanvas) || e.OriginalSource.Equals(sender))
            {
                // Deselect current node when clicking on empty canvas
                ViewModel?.SelectedNode = null;
            }
        }
    }

    private void OnMainCanvasPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (isPanning && ViewModel is not null)
        {
            PointerPoint? pointer = e.GetCurrentPoint(this);
            Point delta = new(pointer.Position.X - panStartPoint.X, pointer.Position.Y - panStartPoint.Y);
            ViewModel.ViewportOffsetX = panStartOffset.X + delta.X;
            ViewModel.ViewportOffsetY = panStartOffset.Y + delta.Y;
            e.Handled = true;
        }
        else if (ViewModel?.PendingConnection is not null)
        {
            // Update pending connection
            Point canvasPoint = e.GetCurrentPoint(MainCanvas).Position;
            ViewModel.UpdatePendingConnection(canvasPoint);
        }
    }

    private void OnMainCanvasPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (isPanning)
        {
            isPanning = false;
            MainCanvas.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        if (ViewModel?.PendingConnection is null)
        {
            return;
        }

        ViewModel.CancelConnection();
        PendingConnectionControl.Visibility = Visibility.Collapsed;
        PendingConnectionControl.DataContext = null;
    }

    private void OnMainCanvasPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        PointerPoint? pointer = e.GetCurrentPoint(this);

        if (pointer.Properties.IsHorizontalMouseWheel || ViewModel is null)
        {
            return;
        }

        int delta = pointer.Properties.MouseWheelDelta;
        bool isCtrlPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
        bool isShiftPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

        if (isCtrlPressed)
        {
            // Ctrl + 滚轮 = 缩放
            double zoomFactor = delta > 0 ? 1.1 : 0.9;

            // Get mouse position relative to canvas
            Point mousePos = e.GetCurrentPoint(MainCanvas).Position;

            // Calculate new scale
            double newScale = ViewModel.ViewportScale * zoomFactor;
            newScale = Math.Max(0.1, Math.Min(5.0, newScale));

            // Snap to 100% when close (within 5% range)
            const double SnapThreshold = 0.05; // 5% threshold
            if (Math.Abs(newScale - 1.0) < SnapThreshold && Math.Abs(ViewModel.ViewportScale - 1.0) >= SnapThreshold)
            {
                newScale = 1.0;
            }

            // Adjust offset to zoom toward mouse position
            double scaleChange = newScale / ViewModel.ViewportScale;
            ViewModel.ViewportOffsetX = mousePos.X - (mousePos.X - ViewModel.ViewportOffsetX) * scaleChange;
            ViewModel.ViewportOffsetY = mousePos.Y - (mousePos.Y - ViewModel.ViewportOffsetY) * scaleChange;
            ViewModel.ViewportScale = newScale;
        }
        else if (isShiftPressed)
        {
            // Shift + 滚轮 = 左右平移
            double panSpeed = 1.0;
            ViewModel.ViewportOffsetX += delta * panSpeed / 10.0;
        }
        else
        {
            // 单纯滚轮 = 上下平移
            double panSpeed = 1.0;
            ViewModel.ViewportOffsetY += delta * panSpeed / 10.0;
        }

        e.Handled = true;
    }

    private void OnMainCanvasDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        Point position = e.GetPosition(MainCanvas);

        // Check if double-tapped on a node or connection
        DependencyObject? originalSource = e.OriginalSource as DependencyObject;
        bool isOverNode = false;

        // Walk up the visual tree to see if we're over a NodeControl
        DependencyObject? current = originalSource;
        while (current != null && current != MainCanvas)
        {
            if (current is NodeControl)
            {
                isOverNode = true;
                break;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        // Only create node if not over an existing node
        if (!isOverNode)
        {
            ViewModel.AddNode(position);
        }

        e.Handled = true;
    }

    // Public methods for external access
    public void AddNode()
    {
        ViewModel?.AddNode(new(100, 100));
    }

    public void ZoomIn()
    {
        if (ViewModel is not null)
        {
            ViewModel.ViewportScale *= 1.2;
        }
    }

    public void ZoomOut()
    {
        if (ViewModel is not null)
        {
            ViewModel.ViewportScale /= 1.2;
        }
    }

    public void ResetView()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.ViewportOffsetX = 0;
        ViewModel.ViewportOffsetY = 0;
        ViewModel.ViewportScale = 1.0;
    }

    public async System.Threading.Tasks.Task<StorageFile?> SaveAsync()
    {
        if (ViewModel is null)
        {
            return null;
        }

        try
        {
            Window window = GetWindow();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFile? file = await SerializationHelper.PickSaveFileAsync(hwnd);

            if (file == null)
            {
                return null;
            }

            EditorStateModel state = ViewModel.GetEditorState();
            await SerializationHelper.SaveToFileAsync(state, file);
            return file;
        }
        catch (Exception ex)
        {
            // Show error dialog
            ContentDialog dialog = new()
            {
                Title = "Save Error",
                Content = $"Failed to save: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
            return null;
        }
    }

    public async System.Threading.Tasks.Task<StorageFile?> LoadAsync()
    {
        if (ViewModel is null)
        {
            return null;
        }

        try
        {
            Window window = GetWindow();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFile? file = await SerializationHelper.PickLoadFileAsync(hwnd);

            if (file == null)
            {
                return null;
            }

            EditorStateModel? state = await SerializationHelper.LoadFromFileAsync(file);
            if (state is not null)
            {
                ViewModel.LoadEditorState(state);

                // Wait for UI to render and update port positions
                await System.Threading.Tasks.Task.Delay(100);
                UpdateAllPortPositions();
                ViewModel.UpdateConnectionPositions();
            }

            return file;
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new()
            {
                Title = "Load Error",
                Content = $"Failed to load: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
            return null;
        }
    }

    private Window GetWindow()
    {
        // In WinUI 3, get the window from App
        return (Application.Current as App)?.Window!;
    }
}