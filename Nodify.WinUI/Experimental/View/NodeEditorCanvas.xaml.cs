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
using Nodify.WinUI.Experimental.Model;

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
        }
    }

    public NodeEditorCanvas()
    {
        InitializeComponent();
    }

    private static IEnumerable<PortControl> FindPortControls(DependencyObject parent)
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

        // Subscribe to selection changes
        node.PropertyChanged += OnNodeViewModelPropertyChanged;

        // Subscribe to port events
        nodeControl.Loaded += OnNodeControlLoaded;

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

    private void OnNodeViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NodeViewModel node)
        {
            return;
        }

        if (e.PropertyName is nameof(NodeViewModel.IsSelected) && node.IsSelected && ViewModel?.SelectedNode != node)
        {
            ViewModel.SelectedNode = node;
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
        foreach (PortControl port in FindPortControls(nodeControl))
        {
            port.ConnectionStarted += OnPortConnectionStarted;
            port.ConnectionCompleted += OnPortConnectionCompleted;
        }
    }

    private void UpdateNodePortPositions(NodeControl nodeControl)
    {
        nodeControl.UpdatePortPositions();
        ViewModel?.UpdateConnectionPositions();
    }

    private void AddConnectionControl(ConnectionViewModel connection)
    {
        ConnectionControl connectionControl = new() { DataContext = connection };
        connectionControl.ConnectionRemoved += OnConnectionControlConnectionRemoved;
        ConnectionsCanvas.Children.Add(connectionControl);
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

    private void OnPortConnectionStarted(object? sender, PortViewModel port)
    {
        connectionStartPort = port;
        ViewModel?.StartConnection(port);

        if (ViewModel?.PendingConnection == null)
        {
            return;
        }

        PendingConnectionControl.DataContext = ViewModel.PendingConnection;
        PendingConnectionControl.Visibility = Visibility.Visible;
    }

    private void OnPortConnectionCompleted(object? sender, PortViewModel? port)
    {
        if (connectionStartPort is not null && port is not null)
        {
            ViewModel?.CompleteConnection(port);
        }

        connectionStartPort = null;
        PendingConnectionControl.Visibility = Visibility.Collapsed;
        PendingConnectionControl.DataContext = null;
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
            if (e.OriginalSource == MainCanvas || e.OriginalSource == sender)
            {
                // Deselect current node when clicking on empty canvas
                if (ViewModel?.SelectedNode != null)
                {
                    ViewModel.SelectedNode = null;
                }
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
        double zoomFactor = delta > 0 ? 1.1 : 0.9;

        // Get mouse position relative to canvas
        Point mousePos = e.GetCurrentPoint(MainCanvas).Position;

        // Calculate new scale
        double newScale = ViewModel.ViewportScale * zoomFactor;
        newScale = Math.Max(0.1, Math.Min(5.0, newScale));

        // Snap to 100% when close (within 5% range)
        const double snapThreshold = 0.05; // 5% threshold
        if (Math.Abs(newScale - 1.0) < snapThreshold && Math.Abs(ViewModel.ViewportScale - 1.0) >= snapThreshold)
        {
            newScale = 1.0;
        }

        // Adjust offset to zoom toward mouse position
        double scaleChange = newScale / ViewModel.ViewportScale;
        ViewModel.ViewportOffsetX = mousePos.X - (mousePos.X - ViewModel.ViewportOffsetX) * scaleChange;
        ViewModel.ViewportOffsetY = mousePos.Y - (mousePos.Y - ViewModel.ViewportOffsetY) * scaleChange;
        ViewModel.ViewportScale = newScale;

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

    private void OnAddNodeButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel?.AddNode(new(100, 100));
    }

    private void OnZoomInButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel?.ViewportScale *= 1.2;
    }

    private void OnZoomOutButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel?.ViewportScale /= 1.2;
    }

    private void OnResetViewButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.ViewportOffsetX = 0;
        ViewModel.ViewportOffsetY = 0;
        ViewModel.ViewportScale = 1.0;
    }

    private async void OnSaveButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        try
        {
            Window window = GetWindow();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFile? file = await SerializationHelper.PickSaveFileAsync(hwnd);

            if (file == null)
            {
                return;
            }

            EditorStateModel state = ViewModel.GetEditorState();
            await SerializationHelper.SaveToFileAsync(state, file);
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
        }
    }

    private async void OnLoadButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        try
        {
            Window window = GetWindow();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFile? file = await SerializationHelper.PickLoadFileAsync(hwnd);

            if (file == null)
            {
                return;
            }

            EditorStateModel? state = await SerializationHelper.LoadFromFileAsync(file);
            if (state is not null)
            {
                ViewModel.LoadEditorState(state);
            }
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
        }
    }

    private Window GetWindow()
    {
        // In WinUI 3, get the window from App
        return (Application.Current as App)?.Window!;
    }
}