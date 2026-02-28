using System;
using Nodify.WinUI.Experimental.Common;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel
{
    /// <summary>
    /// ViewModel for a connection between two ports
    /// </summary>
    public class ConnectionViewModel : ObservableObject
    {
        private readonly ConnectionModel _model;
        private PortViewModel _sourcePort;
        private PortViewModel _targetPort;
        private Point _sourcePoint;
        private Point _targetPoint;
        private bool _isSelected;

        public Guid Id => _model.Id;

        public PortViewModel SourcePort
        {
            get => _sourcePort;
            set
            {
                if (_sourcePort != value)
                {
                    _sourcePort = value;
                    if (_sourcePort != null)
                    {
                        _model.SourcePortId = _sourcePort.Id;
                        _model.SourceNodeId = _sourcePort.NodeId;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public PortViewModel TargetPort
        {
            get => _targetPort;
            set
            {
                if (_targetPort != value)
                {
                    _targetPort = value;
                    if (_targetPort != null)
                    {
                        _model.TargetPortId = _targetPort.Id;
                        _model.TargetNodeId = _targetPort.NodeId;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public Point SourcePoint
        {
            get => _sourcePoint;
            set => SetProperty(ref _sourcePoint, value);
        }

        public Point TargetPoint
        {
            get => _targetPoint;
            set => SetProperty(ref _targetPoint, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ConnectionViewModel(ConnectionModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public ConnectionViewModel(PortViewModel sourcePort, PortViewModel targetPort)
        {
            _model = new ConnectionModel();
            SourcePort = sourcePort;
            TargetPort = targetPort;
        }

        public void UpdatePoints()
        {
            if (SourcePort != null)
                SourcePoint = SourcePort.Position;
            if (TargetPort != null)
                TargetPoint = TargetPort.Position;
        }

        public ConnectionModel GetModel() => _model;
    }
}
