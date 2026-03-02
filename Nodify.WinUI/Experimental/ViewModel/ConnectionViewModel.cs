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
                field = value;
                if (field != null)
                {
                    model.SourcePortId = field.Id;
                    model.SourceNodeId = field.NodeId;
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
                field = value;
                if (field != null)
                {
                    model.TargetPortId = field.Id;
                    model.TargetNodeId = field.NodeId;
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
        }

        if (TargetPort is not null)
        {
            TargetPoint = TargetPort.Position;
        }
    }

    public ConnectionModel GetModel()
    {
        return model;
    }
}