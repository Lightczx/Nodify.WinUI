using Microsoft.UI.Xaml.Controls;
using Nodify.WinUI.Experimental.Model;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental
{
    public sealed partial class SamplePage : Page
    {
        public SamplePage()
        {
            this.InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            var viewModel = new NodeEditorViewModel();
            
            // Create some sample nodes
            CreateSampleNode(viewModel, "Start", new Point(100, 100), "Entry point", 1, 2);
            CreateSampleNode(viewModel, "Process", new Point(400, 150), "Processing...", 2, 2);
            CreateSampleNode(viewModel, "Output", new Point(700, 100), "Result", 2, 1);

            NodeEditor.ViewModel = viewModel;
        }

        private void CreateSampleNode(NodeEditorViewModel viewModel, string title, Point position, 
            string content, int inputCount, int outputCount)
        {
            var nodeModel = new NodeModel(title, position.X, position.Y)
            {
                Content = content
            };

            for (int i = 0; i < inputCount; i++)
            {
                nodeModel.InputPorts.Add(new PortModel($"In {i + 1}", PortDirection.Input) 
                { 
                    NodeId = nodeModel.Id 
                });
            }

            for (int i = 0; i < outputCount; i++)
            {
                nodeModel.OutputPorts.Add(new PortModel($"Out {i + 1}", PortDirection.Output) 
                { 
                    NodeId = nodeModel.Id 
                });
            }

            var nodeViewModel = new NodeViewModel(nodeModel);
            viewModel.Nodes.Add(nodeViewModel);
        }

        private void ResetZoom_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (NodeEditor?.ViewModel != null)
            {
                NodeEditor.ViewModel.ViewportScale = 1.0;
            }
        }

        private void DeleteNode_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (NodeEditor?.ViewModel?.SelectedNode != null)
            {
                NodeEditor.ViewModel.DeleteNode(NodeEditor.ViewModel.SelectedNode);
                NodeEditor.ViewModel.SelectedNode = null;
            }
        }

        private void AddInputPort_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (NodeEditor?.ViewModel?.SelectedNode != null)
            {
                int count = NodeEditor.ViewModel.SelectedNode.InputPorts.Count + 1;
                NodeEditor.ViewModel.SelectedNode.AddInputPort($"Input {count}");
            }
        }

        private void AddOutputPort_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (NodeEditor?.ViewModel?.SelectedNode != null)
            {
                int count = NodeEditor.ViewModel.SelectedNode.OutputPorts.Count + 1;
                NodeEditor.ViewModel.SelectedNode.AddOutputPort($"Output {count}");
            }
        }
    }
}
