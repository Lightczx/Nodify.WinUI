using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel;

/// <summary>
/// ViewModel for a connection between two ports
/// </summary>
public sealed partial class ConnectionViewModel : ObservableObject
{
    private readonly ConnectionModel model;

    public ConnectionViewModel(ConnectionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        this.model = model;
    }

    public ConnectionViewModel(PortViewModel sourcePort, PortViewModel targetPort)
    {
        model = ConnectionModel.Create();
        SourcePort = sourcePort;
        TargetPort = targetPort;
    }

    public Guid Id { get => model.Id; }

    public PortViewModel? SourcePort
    {
        get;
        set
        {
            if (field != value)
            {
                // Unsubscribe from old port
                if (field != null)
                {
                    field.PropertyChanged -= OnPortPropertyChanged;
                }

                field = value;
                
                if (field != null)
                {
                    model.SourcePortId = field.Id;
                    model.SourceNodeId = field.NodeId;
                    // Subscribe to new port
                    field.PropertyChanged += OnPortPropertyChanged;
                }
                OnPropertyChanged();
            }
        }
    }

    public PortViewModel? TargetPort
    {
        get;
        set
        {
            if (field != value)
            {
                // Unsubscribe from old port
                if (field != null)
                {
                    field.PropertyChanged -= OnPortPropertyChanged;
                }

                field = value;
                
                if (field != null)
                {
                    model.TargetPortId = field.Id;
                    model.TargetNodeId = field.NodeId;
                    // Subscribe to new port
                    field.PropertyChanged += OnPortPropertyChanged;
                }
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    public partial Point SourcePoint { get; set; }

    [ObservableProperty]
    public partial Point TargetPoint { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public void UpdatePoints()
    {
        if (SourcePort is not null)
        {
            SourcePoint = SourcePort.Position;
            System.Diagnostics.Debug.WriteLine($"[ConnectionViewModel] UpdatePoints - Source: {SourcePort.Name} at ({SourcePort.Position.X:F2}, {SourcePort.Position.Y:F2})");
        }

        if (TargetPort is not null)
        {
            TargetPoint = TargetPort.Position;
            System.Diagnostics.Debug.WriteLine($"[ConnectionViewModel] UpdatePoints - Target: {TargetPort.Name} at ({TargetPort.Position.X:F2}, {TargetPort.Position.Y:F2})");
        }
    }

    private void OnPortPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PortViewModel.Position))
        {
            UpdatePoints();
        }
    }

    public ConnectionModel GetModel()
    {
        return model;
    }
}