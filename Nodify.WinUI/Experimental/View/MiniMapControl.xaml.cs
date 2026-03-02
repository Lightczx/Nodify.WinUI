using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;
using Microsoft.UI;

namespace Nodify.WinUI.Experimental.View;

public sealed partial class MiniMapControl : UserControl
{
    private const double MiniMapScale = 0.1;
    private bool isDraggingViewport;

    public MiniMapControl()
    {
        InitializeComponent();
    }

    public NodeEditorViewModel? ViewModel
    {
        get => DataContext as NodeEditorViewModel;
        set
        {
            NodeEditorViewModel? @field = ViewModel;
            if (@field is not null)
            {
                @field.Nodes.CollectionChanged -= OnViewModelNodesCollectionChanged;
                @field.PropertyChanged -= OnViewModelPropertyChanged;
                
                // Unsubscribe from all existing nodes
                foreach (NodeViewModel node in @field.Nodes)
                {
                    node.PropertyChanged -= OnNodePropertyChanged;
                }
            }

            @field = value;
            DataContext = value;

            if (@field is not null)
            {
                @field.Nodes.CollectionChanged += OnViewModelNodesCollectionChanged;
                @field.PropertyChanged += OnViewModelPropertyChanged;
                
                // Subscribe to all existing nodes
                foreach (NodeViewModel node in @field.Nodes)
                {
                    node.PropertyChanged += OnNodePropertyChanged;
                }
                
                UpdateMiniMap();
            }
        }
    }

    private void OnViewModelNodesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to new nodes
        if (e.NewItems is not null)
        {
            foreach (NodeViewModel node in e.NewItems)
            {
                node.PropertyChanged += OnNodePropertyChanged;
            }
        }

        // Unsubscribe from removed nodes
        if (e.OldItems is not null)
        {
            foreach (NodeViewModel node in e.OldItems)
            {
                node.PropertyChanged -= OnNodePropertyChanged;
            }
        }

        UpdateMiniMap();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName
            is nameof(NodeEditorViewModel.ViewportOffsetX)
            or nameof(NodeEditorViewModel.ViewportOffsetY)
            or nameof(NodeEditorViewModel.ViewportScale)
            or nameof(NodeEditorViewModel.ViewportWidth)
            or nameof(NodeEditorViewModel.ViewportHeight))
        {
            UpdateViewportRect();
        }
    }

    private void OnNodePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName
            is nameof(NodeViewModel.X)
            or nameof(NodeViewModel.Y)
            or nameof(NodeViewModel.Width)
            or nameof(NodeViewModel.Height))
        {
            UpdateMiniMap();
        }
    }

    private void UpdateMiniMap()
    {
        if (ViewModel is null)
        {
            return;
        }

        MiniMapCanvas.Children.Clear();

        // Draw nodes as small rectangles
        foreach (NodeViewModel node in ViewModel.Nodes)
        {
            Rectangle rect = new()
            {
                Width = node.Width * MiniMapScale,
                Height = node.Height * MiniMapScale,
                Fill = new SolidColorBrush(Colors.Gray),
                Opacity = 0.7,
            };

            Canvas.SetLeft(rect, node.X * MiniMapScale);
            Canvas.SetTop(rect, node.Y * MiniMapScale);

            MiniMapCanvas.Children.Add(rect);
        }

        // Re-add viewport rect on top
        if (!MiniMapCanvas.Children.Contains(ViewportRect))
        {
            MiniMapCanvas.Children.Add(ViewportRect);
        }

        UpdateViewportRect();
    }

    private void UpdateViewportRect()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewportRect.Width = (ViewModel.ViewportWidth / ViewModel.ViewportScale) * MiniMapScale;
        ViewportRect.Height = (ViewModel.ViewportHeight / ViewModel.ViewportScale) * MiniMapScale;

        Canvas.SetLeft(ViewportRect, -ViewModel.ViewportOffsetX * MiniMapScale);
        Canvas.SetTop(ViewportRect, -ViewModel.ViewportOffsetY * MiniMapScale);
    }

    private void OnViewportRectPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        isDraggingViewport = true;
        ViewportRect.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnViewportRectPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!isDraggingViewport || ViewModel is null)
        {
            return;
        }

        Point position = e.GetCurrentPoint(MiniMapCanvas).Position;

        ViewModel.ViewportOffsetX = -(position.X - ViewportRect.Width / 2) / MiniMapScale;
        ViewModel.ViewportOffsetY = -(position.Y - ViewportRect.Height / 2) / MiniMapScale;

        e.Handled = true;
    }

    private void OnViewportRectPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!isDraggingViewport)
        {
            return;
        }

        isDraggingViewport = false;
        ViewportRect.ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }
}