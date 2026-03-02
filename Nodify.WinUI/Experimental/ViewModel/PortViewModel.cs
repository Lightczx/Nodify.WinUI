using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel;

/// <summary>
/// ViewModel for a port
/// </summary>
public sealed partial class PortViewModel : ObservableObject
{
    private readonly PortModel model;

    public PortViewModel(PortModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        this.model = model;
    }

    public Guid Id { get => model.Id; }

    public string Name
    {
        get => model.Name;
        set => SetProperty(model.Name, value, model, static (m, v) => m.Name = v);
    }

    public PortDirection Direction
    {
        get => model.Direction;
        set => SetProperty(model.Direction, value, model, static (m, v) => m.Direction = v);
    }

    public PortType Type
    {
        get => model.Type;
        set => SetProperty(model.Type, value, model, static (m, v) => m.Type = v);
    }

    public Guid NodeId
    {
        get => model.NodeId;
        set => model.NodeId = value;
    }

    /// <summary>
    /// Absolute position of the port in canvas coordinates (updated by UI)
    /// </summary>
    [ObservableProperty]
    public partial Point Position { get; set; }

    public NodeViewModel? ParentNode { get; set; }

    public PortModel GetModel()
    {
        return model;
    }
}