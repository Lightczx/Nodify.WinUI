using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Nodify.WinUI.Experimental.Common;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel
{
    /// <summary>
    /// Main ViewModel for the Node Editor
    /// </summary>
    public class NodeEditorViewModel : ObservableObject
    {
        private double _viewportOffsetX;
        private double _viewportOffsetY;
        private double _viewportScale = 1.0;
        private ConnectionViewModel _pendingConnection;
        private bool _isPanning;

        public ObservableCollection<NodeViewModel> Nodes { get; }
        public ObservableCollection<ConnectionViewModel> Connections { get; }

        public double ViewportOffsetX
        {
            get => _viewportOffsetX;
            set => SetProperty(ref _viewportOffsetX, value);
        }

        public double ViewportOffsetY
        {
            get => _viewportOffsetY;
            set => SetProperty(ref _viewportOffsetY, value);
        }

        public double ViewportScale
        {
            get => _viewportScale;
            set
            {
                if (value > 0.1 && value < 5.0)
                {
                    SetProperty(ref _viewportScale, value);
                }
            }
        }

        public ConnectionViewModel PendingConnection
        {
            get => _pendingConnection;
            set => SetProperty(ref _pendingConnection, value);
        }

        public bool IsPanning
        {
            get => _isPanning;
            set => SetProperty(ref _isPanning, value);
        }

        public ICommand AddNodeCommand { get; }
        public ICommand DeleteNodeCommand { get; }
        public ICommand DeleteConnectionCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetViewCommand { get; }

        public NodeEditorViewModel()
        {
            Nodes = new ObservableCollection<NodeViewModel>();
            Connections = new ObservableCollection<ConnectionViewModel>();

            AddNodeCommand = new RelayCommand<Point>(AddNode);
            DeleteNodeCommand = new RelayCommand<NodeViewModel>(DeleteNode);
            DeleteConnectionCommand = new RelayCommand<ConnectionViewModel>(DeleteConnection);
            ZoomInCommand = new RelayCommand(() => ViewportScale *= 1.2);
            ZoomOutCommand = new RelayCommand(() => ViewportScale /= 1.2);
            ResetViewCommand = new RelayCommand(ResetView);
        }

        public void AddNode(Point position)
        {
            var model = new NodeModel($"Node {Nodes.Count + 1}", position.X, position.Y);
            var viewModel = new NodeViewModel(model);
            
            // Add some default ports
            viewModel.AddInputPort("Input 1");
            viewModel.AddOutputPort("Output 1");
            
            Nodes.Add(viewModel);
        }

        public void DeleteNode(NodeViewModel node)
        {
            if (node == null) return;

            // Remove all connections to this node
            var connectionsToRemove = Connections
                .Where(c => c.SourcePort?.ParentNode == node || c.TargetPort?.ParentNode == node)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                Connections.Remove(connection);
            }

            Nodes.Remove(node);
        }

        public void DeleteConnection(ConnectionViewModel connection)
        {
            if (connection != null)
            {
                Connections.Remove(connection);
            }
        }

        public void StartConnection(PortViewModel port)
        {
            if (port == null) return;

            var model = new ConnectionModel();
            PendingConnection = new ConnectionViewModel(model);

            if (port.Direction == PortDirection.Output)
            {
                PendingConnection.SourcePort = port;
            }
            else
            {
                PendingConnection.TargetPort = port;
            }
        }

        public void UpdatePendingConnection(Point point)
        {
            if (PendingConnection == null) return;

            if (PendingConnection.SourcePort != null)
            {
                PendingConnection.SourcePoint = PendingConnection.SourcePort.Position;
                PendingConnection.TargetPoint = point;
            }
            else if (PendingConnection.TargetPort != null)
            {
                PendingConnection.SourcePoint = point;
                PendingConnection.TargetPoint = PendingConnection.TargetPort.Position;
            }
        }

        public void CompleteConnection(PortViewModel port)
        {
            if (PendingConnection == null || port == null) return;

            // Validate connection
            PortViewModel sourcePort = null;
            PortViewModel targetPort = null;

            if (PendingConnection.SourcePort != null && port.Direction == PortDirection.Input)
            {
                sourcePort = PendingConnection.SourcePort;
                targetPort = port;
            }
            else if (PendingConnection.TargetPort != null && port.Direction == PortDirection.Output)
            {
                sourcePort = port;
                targetPort = PendingConnection.TargetPort;
            }

            // Prevent connecting to the same node
            if (sourcePort != null && targetPort != null && sourcePort.ParentNode != targetPort.ParentNode)
            {
                // Check if connection already exists
                var exists = Connections.Any(c => 
                    c.SourcePort == sourcePort && c.TargetPort == targetPort);

                if (!exists)
                {
                    var connection = new ConnectionViewModel(sourcePort, targetPort);
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
            foreach (var connection in Connections)
            {
                connection.UpdatePoints();
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
            return new EditorStateModel
            {
                Nodes = Nodes.Select(n => n.GetModel()).ToList(),
                Connections = Connections.Select(c => c.GetModel()).ToList(),
                ViewportOffsetX = ViewportOffsetX,
                ViewportOffsetY = ViewportOffsetY,
                ViewportScale = ViewportScale
            };
        }

        public void LoadEditorState(EditorStateModel state)
        {
            if (state == null) return;

            Nodes.Clear();
            Connections.Clear();

            // Load nodes
            var nodeViewModels = state.Nodes.Select(n => new NodeViewModel(n)).ToList();
            foreach (var node in nodeViewModels)
            {
                Nodes.Add(node);
            }

            // Load connections
            foreach (var connectionModel in state.Connections)
            {
                var sourceNode = nodeViewModels.FirstOrDefault(n => n.Id == connectionModel.SourceNodeId);
                var targetNode = nodeViewModels.FirstOrDefault(n => n.Id == connectionModel.TargetNodeId);

                if (sourceNode != null && targetNode != null)
                {
                    var sourcePort = sourceNode.OutputPorts.FirstOrDefault(p => p.Id == connectionModel.SourcePortId);
                    var targetPort = targetNode.InputPorts.FirstOrDefault(p => p.Id == connectionModel.TargetPortId);

                    if (sourcePort != null && targetPort != null)
                    {
                        var connection = new ConnectionViewModel(connectionModel)
                        {
                            SourcePort = sourcePort,
                            TargetPort = targetPort
                        };
                        Connections.Add(connection);
                    }
                }
            }

            ViewportOffsetX = state.ViewportOffsetX;
            ViewportOffsetY = state.ViewportOffsetY;
            ViewportScale = state.ViewportScale;
        }
    }
}
