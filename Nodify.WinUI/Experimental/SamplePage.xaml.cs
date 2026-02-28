using Microsoft.UI.Xaml.Controls;
using Nodify.WinUI.Experimental.Model;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental;

public sealed partial class SamplePage : Page
{
    public SamplePage()
    {
        InitializeComponent();
        InitializeEditor();
    }

    private void InitializeEditor()
    {
        NodeEditorViewModel viewModel = new();
        NodeEditor.ViewModel = viewModel;
    }

    private void OnResetZoomClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (NodeEditor?.ViewModel != null)
        {
            NodeEditor.ViewModel.ViewportScale = 1.0;
        }
    }

    private void OnDeleteNodeClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (NodeEditor?.ViewModel?.SelectedNode != null)
        {
            NodeEditor.ViewModel.DeleteNode(NodeEditor.ViewModel.SelectedNode);
            NodeEditor.ViewModel.SelectedNode = null;
        }
    }

    private void OnAddInputPortClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (NodeEditor?.ViewModel?.SelectedNode != null)
        {
            int count = NodeEditor.ViewModel.SelectedNode.InputPorts.Count + 1;
            NodeEditor.ViewModel.SelectedNode.AddInputPort($"Input {count}");
        }
    }

    private void OnAddOutputPortClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (NodeEditor?.ViewModel?.SelectedNode != null)
        {
            int count = NodeEditor.ViewModel.SelectedNode.OutputPorts.Count + 1;
            NodeEditor.ViewModel.SelectedNode.AddOutputPort($"Output {count}");
        }
    }
}