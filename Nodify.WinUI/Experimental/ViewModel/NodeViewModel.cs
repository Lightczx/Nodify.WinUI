using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.ViewModel;

/// <summary>
/// ViewModel for a node
/// </summary>
public sealed partial class NodeViewModel : ObservableObject
{
    private readonly NodeModel model;

    public NodeViewModel(NodeModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        this.model = model;

        InputPorts = new(model.InputPorts.Select(p => new PortViewModel(p) { ParentNode = this, NodeId = Id }));

        OutputPorts = new(model.OutputPorts.Select(p => new PortViewModel(p) { ParentNode = this, NodeId = Id }));

        AddInputPortCommand = new RelayCommand(() => AddInputPort($"Input {InputPorts.Count + 1}"));
        AddOutputPortCommand = new RelayCommand(() => AddOutputPort($"Output {OutputPorts.Count + 1}"));
        RemovePortCommand = new RelayCommand<PortViewModel>(RemovePort);
    }

    public Guid Id { get => model.Id; }

    public string Title
    {
        get => model.Title;
        set => SetProperty(model.Title, value, model, static (m, v) => m.Title = v);
    }

    public string Content
    {
        get => model.Content;
        set => SetProperty(model.Content, value, model, static (m, v) => m.Content = v);
    }

    public double X
    {
        get => model.PositionX;
        set => SetProperty(model.PositionX, value, model, static (m, v) => m.PositionX = v);
    }

    public double Y
    {
        get => model.PositionY;
        set => SetProperty(model.PositionY, value, model, static (m, v) => m.PositionY = v);
    }

    public Point Position
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public double Width
    {
        get => model.Width;
        set => SetProperty(model.Width, value, model, static (m, v) => m.Width = v);
    }

    public double Height
    {
        get => model.Height;
        set => SetProperty(model.Height, value, model, static (m, v) => m.Height = v);
    }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public ObservableCollection<PortViewModel> InputPorts { get; }

    public ObservableCollection<PortViewModel> OutputPorts { get; }

    public ICommand AddInputPortCommand { get; }

    public ICommand AddOutputPortCommand { get; }

    public ICommand RemovePortCommand { get; }

    public void AddInputPort(string name, PortType type = PortType.Default)
    {
        PortModel portModel = PortModel.Create(name, PortDirection.Input, type, Id);
        model.InputPorts.Add(portModel);
        InputPorts.Add(new(portModel) { ParentNode = this, NodeId = Id });
    }

    public void AddOutputPort(string name, PortType type = PortType.Default)
    {
        PortModel portModel = PortModel.Create(name, PortDirection.Output, type, Id);
        model.OutputPorts.Add(portModel);
        OutputPorts.Add(new(portModel) { ParentNode = this, NodeId = Id });
    }

    public void RemovePort(PortViewModel? port)
    {
        if (port is null)
        {
            return;
        }

        if (port.Direction is PortDirection.Input)
        {
            PortModel? input = model.InputPorts.FirstOrDefault(p => p.Id == port.Id);
            if (input is not null)
            {
                model.InputPorts.Remove(input);
                InputPorts.Remove(port);
            }
        }
        else
        {
            PortModel? output = model.OutputPorts.FirstOrDefault(p => p.Id == port.Id);
            if (output is not null)
            {
                model.OutputPorts.Remove(output);
                OutputPorts.Remove(port);
            }
        }
    }

    public NodeModel GetModel()
    {
        return model;
    }
}