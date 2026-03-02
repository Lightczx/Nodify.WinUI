using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel;

/// <summary>
/// Main ViewModel for the Node Editor
/// </summary>
public sealed partial class NodeEditorViewModel : ObservableObject
{
    private bool _isShiftingNodes = false;

    public NodeEditorViewModel()
    {
        Nodes = [];
        Connections = [];

        // Subscribe to collection changes to monitor node additions/removals
        Nodes.CollectionChanged += OnNodesCollectionChanged;

        AddNodeCommand = new RelayCommand<Point>(AddNode);
        DeleteNodeCommand = new RelayCommand<NodeViewModel>(DeleteNode);
        DeleteSelectedNodeCommand = new RelayCommand(DeleteSelectedNode, () => SelectedNode != null);
        DeleteConnectionCommand = new RelayCommand<ConnectionViewModel>(DeleteConnection);
        ZoomInCommand = new RelayCommand(() => ViewportScale *= 1.2);
        ZoomOutCommand = new RelayCommand(() => ViewportScale /= 1.2);
        ResetViewCommand = new RelayCommand(ResetView);

        // Initialize canvas size
        UpdateCanvasSize();
    }

    public ObservableCollection<NodeViewModel> Nodes { get; }

    public ObservableCollection<ConnectionViewModel> Connections { get; }

    [ObservableProperty]
    public partial double ViewportOffsetX { get; set; }

    [ObservableProperty]
    public partial double ViewportOffsetY { get; set; }

    [ObservableProperty]
    public partial double ViewportWidth { get; set; } = 800;

    [ObservableProperty]
    public partial double ViewportHeight { get; set; } = 600;

    [ObservableProperty]
    public partial double CanvasWidth { get; set; } = 200;

    [ObservableProperty]
    public partial double CanvasHeight { get; set; } = 200;

    public double ViewportScale
    {
        get;
        set
        {
            if (value is > 0.1 and < 5.0)
            {
                SetProperty(ref field, value);
            }
        }
    } = 1.0;

    [ObservableProperty]
    public partial ConnectionViewModel? PendingConnection { get; set; }

    [ObservableProperty]
    public partial bool IsPanning { get; set; }

    [ObservableProperty]
    public partial NodeViewModel? SelectedNode { get; set; }

    [ObservableProperty]
    public partial string? CurrentFileName { get; set; } = "未命名";

    partial void OnSelectedNodeChanging(NodeViewModel? value)
    {
        // Deselect the old node
        if (SelectedNode != null)
        {
            SelectedNode.IsSelected = false;
        }
    }

    partial void OnSelectedNodeChanged(NodeViewModel? value)
    {
        // Select the new node
        if (value != null)
        {
            value.IsSelected = true;
        }

        // Notify commands that depend on selected node
        (DeleteSelectedNodeCommand as RelayCommand)?.NotifyCanExecuteChanged();
    }

    public ICommand AddNodeCommand { get; }

    public ICommand DeleteNodeCommand { get; }

    public ICommand DeleteSelectedNodeCommand { get; }

    public ICommand DeleteConnectionCommand { get; }

    public ICommand ZoomInCommand { get; }

    public ICommand ZoomOutCommand { get; }

    public ICommand ResetViewCommand { get; }

    public void AddNode(Point position)
    {
        NodeModel model = NodeModel.Create($"Node {Nodes.Count + 1}", string.Empty, position.X, position.Y);
        NodeViewModel viewModel = new(model);

        // Add some default ports
        viewModel.AddInputPort("Input 1");
        viewModel.AddOutputPort("Output 1");

        Nodes.Add(viewModel);
        UpdateCanvasSize();
    }

    public void DeleteNode(NodeViewModel? node)
    {
        if (node is null)
        {
            return;
        }

        // Remove all connections to this node
        List<ConnectionViewModel> connectionsToRemove = Connections
            .Where(c => c.SourcePort?.ParentNode == node || c.TargetPort?.ParentNode == node)
            .ToList();

        foreach (ConnectionViewModel connection in connectionsToRemove)
        {
            Connections.Remove(connection);
        }

        // Clear selection if this node is selected
        if (SelectedNode == node)
        {
            SelectedNode = null;
        }

        Nodes.Remove(node);
        UpdateCanvasSize();
    }

    public void DeleteSelectedNode()
    {
        if (SelectedNode != null)
        {
            DeleteNode(SelectedNode);
        }
    }

    public void DeleteConnection(ConnectionViewModel? connection)
    {
        if (connection is not null)
        {
            Connections.Remove(connection);
        }
    }

    public void StartConnection(PortViewModel? port)
    {
        if (port is null)
        {
            return;
        }

        ConnectionModel model = ConnectionModel.Create();
        PendingConnection = new(model)
        {
            IsSelected = false // Ensure pending connection is not selected
        };

        if (port.Direction == PortDirection.Output)
        {
            PendingConnection.SourcePort = port;
        }
        else
        {
            PendingConnection.TargetPort = port;
        }

        PendingConnection.SourcePoint = port.Position;
        PendingConnection.TargetPoint = port.Position; // Start at the same position
    }

    public void UpdatePendingConnection(Point point)
    {
        if (PendingConnection is null)
        {
            return;
        }

        if (PendingConnection.SourcePort is not null)
        {
            PendingConnection.SourcePoint = PendingConnection.SourcePort.Position;
            PendingConnection.TargetPoint = point;
        }
        else if (PendingConnection.TargetPort is not null)
        {
            PendingConnection.SourcePoint = point;
            PendingConnection.TargetPoint = PendingConnection.TargetPort.Position;
        }
    }

    public void CompleteConnection(PortViewModel? port)
    {
        if (PendingConnection is null || port is null)
        {
            return;
        }

        // Validate connection
        PortViewModel? sourcePort = null;
        PortViewModel? targetPort = null;

        if (PendingConnection.SourcePort is not null && port.Direction == PortDirection.Input)
        {
            sourcePort = PendingConnection.SourcePort;
            targetPort = port;
        }
        else if (PendingConnection.TargetPort is not null && port.Direction == PortDirection.Output)
        {
            sourcePort = port;
            targetPort = PendingConnection.TargetPort;
        }

        // Prevent connecting to the same node
        if (sourcePort is not null && targetPort is not null && sourcePort.ParentNode != targetPort.ParentNode)
        {
            // Check if connection already exists
            bool exists = Connections.Any(c => c.SourcePort == sourcePort && c.TargetPort == targetPort);

            if (!exists)
            {
                ConnectionViewModel connection = new(sourcePort, targetPort);
                connection.UpdatePoints();
                Connections.Add(connection);
            }
        }

        PendingConnection = null;
    }

    public void CancelConnection()
    {
        PendingConnection = null;
    }

    public void UpdateConnectionPositions()
    {
        foreach (ConnectionViewModel connection in Connections)
        {
            connection.UpdatePoints();
        }
    }

    public void CheckAndShiftNodes()
    {
        if (Nodes.Count == 0)
        {
            return;
        }

        // Find minimum coordinates
        double minX = double.MaxValue;
        double minY = double.MaxValue;

        foreach (NodeViewModel node in Nodes)
        {
            minX = Math.Min(minX, node.X);
            minY = Math.Min(minY, node.Y);
        }

        // Calculate shift needed (with a small margin)
        const double Margin = 48;
        double shiftX = 0;
        double shiftY = 0;

        if (minX < Margin)
        {
            shiftX = Margin - minX;
        }

        if (minY < Margin)
        {
            shiftY = Margin - minY;
        }

        // Only shift if necessary
        if (shiftX == 0 && shiftY == 0)
        {
            return;
        }

        // Set flag to prevent multiple canvas size updates
        _isShiftingNodes = true;

        try
        {
            // Shift all nodes
            foreach (NodeViewModel node in Nodes)
            {
                node.X += shiftX;
                node.Y += shiftY;
            }

            // Adjust viewport offset to maintain visual continuity.
            // Nodes move right by shiftX in canvas space, so the viewport must
            // shift left by shiftX * scale to keep them at the same screen position.
            ViewportOffsetX -= shiftX * ViewportScale;
            ViewportOffsetY -= shiftY * ViewportScale;

            // Update connections
            UpdateConnectionPositions();
        }
        finally
        {
            _isShiftingNodes = false;
            
            // Update canvas size once after all shifts
            UpdateCanvasSize();
        }
    }

    private void ResetView()
    {
        ViewportOffsetX = 0;
        ViewportOffsetY = 0;
        ViewportScale = 1.0;
    }

    public EditorStateModel GetEditorState()
    {
        return EditorStateModel.Create(
            Nodes.Select(n => n.GetModel()).ToList(),
            Connections.Select(c => c.GetModel()).ToList(),
            ViewportOffsetX,
            ViewportOffsetY,
            ViewportScale);
    }

    public void LoadEditorState(EditorStateModel? state)
    {
        if (state is null)
        {
            return;
        }

        Nodes.Clear();
        Connections.Clear();

        // Load nodes
        List<NodeViewModel> nodeViewModels = [.. state.Nodes.Select(n => new NodeViewModel(n))];
        foreach (NodeViewModel node in nodeViewModels)
        {
            Nodes.Add(node);
        }

        // Load connections
        foreach (ConnectionModel connectionModel in state.Connections)
        {
            NodeViewModel? sourceNode = nodeViewModels.FirstOrDefault(n => n.Id == connectionModel.SourceNodeId);
            NodeViewModel? targetNode = nodeViewModels.FirstOrDefault(n => n.Id == connectionModel.TargetNodeId);

            if (sourceNode != null && targetNode != null)
            {
                PortViewModel? sourcePort = sourceNode.OutputPorts.FirstOrDefault(p => p.Id == connectionModel.SourcePortId);
                PortViewModel? targetPort = targetNode.InputPorts.FirstOrDefault(p => p.Id == connectionModel.TargetPortId);

                if (sourcePort != null && targetPort != null)
                {
                    ConnectionViewModel connection = new ConnectionViewModel(connectionModel)
                    {
                        SourcePort = sourcePort,
                        TargetPort = targetPort
                    };
                    connection.UpdatePoints();
                    Connections.Add(connection);
                }
            }
        }

        ViewportOffsetX = state.ViewportOffsetX;
        ViewportOffsetY = state.ViewportOffsetY;
        ViewportScale = state.ViewportScale;

        UpdateCanvasSize();
    }

    private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to new nodes
        if (e.NewItems != null)
        {
            foreach (NodeViewModel node in e.NewItems)
            {
                node.PropertyChanged += OnNodePropertyChanged;
            }
        }

        // Unsubscribe from removed nodes
        if (e.OldItems != null)
        {
            foreach (NodeViewModel node in e.OldItems)
            {
                node.PropertyChanged -= OnNodePropertyChanged;
            }
        }

        UpdateCanvasSize();
    }

    private void OnNodePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Skip updates when shifting nodes to avoid multiple canvas size updates
        if (_isShiftingNodes)
        {
            return;
        }

        // Update canvas size when node position or size changes
        if (e.PropertyName is nameof(NodeViewModel.X) or nameof(NodeViewModel.Y) 
            or nameof(NodeViewModel.Width) or nameof(NodeViewModel.Height))
        {
            UpdateCanvasSize();
        }
    }

    private void UpdateCanvasSize()
    {
        const double DefaultSize = 200;
        const double Margin = 48;

        if (Nodes.Count == 0)
        {
            CanvasWidth = DefaultSize;
            CanvasHeight = DefaultSize;
            return;
        }

        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (NodeViewModel node in Nodes)
        {
            maxX = Math.Max(maxX, node.X + node.Width);
            maxY = Math.Max(maxY, node.Y + node.Height);
        }

        CanvasWidth = maxX + Margin;
        CanvasHeight = maxY + Margin;
    }
}