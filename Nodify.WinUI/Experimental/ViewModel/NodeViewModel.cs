using System;
using System.Collections.ObjectModel;
using System.Linq;
using Nodify.WinUI.Experimental.Common;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel
{
    /// <summary>
    /// ViewModel for a node
    /// </summary>
    public class NodeViewModel : ObservableObject
    {
        private readonly NodeModel _model;
        private bool _isSelected;

        public Guid Id => _model.Id;

        public string Title
        {
            get => _model.Title;
            set
            {
                if (_model.Title != value)
                {
                    _model.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Content
        {
            get => _model.Content;
            set
            {
                if (_model.Content != value)
                {
                    _model.Content = value;
                    OnPropertyChanged();
                }
            }
        }

        public double X
        {
            get => _model.PositionX;
            set
            {
                if (_model.PositionX != value)
                {
                    _model.PositionX = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public double Y
        {
            get => _model.PositionY;
            set
            {
                if (_model.PositionY != value)
                {
                    _model.PositionY = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public Point Position
        {
            get => new Point(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public double Width
        {
            get => _model.Width;
            set
            {
                if (_model.Width != value)
                {
                    _model.Width = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Height
        {
            get => _model.Height;
            set
            {
                if (_model.Height != value)
                {
                    _model.Height = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ObservableCollection<PortViewModel> InputPorts { get; }
        public ObservableCollection<PortViewModel> OutputPorts { get; }

        public NodeViewModel(NodeModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            InputPorts = new ObservableCollection<PortViewModel>(
                model.InputPorts.Select(p => new PortViewModel(p) { ParentNode = this, NodeId = Id })
            );

            OutputPorts = new ObservableCollection<PortViewModel>(
                model.OutputPorts.Select(p => new PortViewModel(p) { ParentNode = this, NodeId = Id })
            );
        }

        public void AddInputPort(string name, PortType type = PortType.Default)
        {
            var portModel = new PortModel(name, PortDirection.Input, type) { NodeId = Id };
            _model.InputPorts.Add(portModel);
            InputPorts.Add(new PortViewModel(portModel) { ParentNode = this, NodeId = Id });
        }

        public void AddOutputPort(string name, PortType type = PortType.Default)
        {
            var portModel = new PortModel(name, PortDirection.Output, type) { NodeId = Id };
            _model.OutputPorts.Add(portModel);
            OutputPorts.Add(new PortViewModel(portModel) { ParentNode = this, NodeId = Id });
        }

        public NodeModel GetModel() => _model;
    }
}
