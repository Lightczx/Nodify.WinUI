using System;
using Nodify.WinUI.Experimental.Common;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel
{
    /// <summary>
    /// ViewModel for a port
    /// </summary>
    public class PortViewModel : ObservableObject
    {
        private readonly PortModel _model;
        private Point _position;

        public Guid Id => _model.Id;

        public string Name
        {
            get => _model.Name;
            set
            {
                if (_model.Name != value)
                {
                    _model.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public PortDirection Direction
        {
            get => _model.Direction;
            set
            {
                if (_model.Direction != value)
                {
                    _model.Direction = value;
                    OnPropertyChanged();
                }
            }
        }

        public PortType Type
        {
            get => _model.Type;
            set
            {
                if (_model.Type != value)
                {
                    _model.Type = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid NodeId
        {
            get => _model.NodeId;
            set => _model.NodeId = value;
        }

        /// <summary>
        /// Absolute position of the port in canvas coordinates (updated by UI)
        /// </summary>
        public Point Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public NodeViewModel ParentNode { get; set; }

        public PortViewModel(PortModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public PortModel GetModel() => _model;
    }
}
