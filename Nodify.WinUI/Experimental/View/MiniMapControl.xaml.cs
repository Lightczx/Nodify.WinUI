using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View
{
    public sealed partial class MiniMapControl : UserControl
    {
        private const double MiniMapScale = 0.1;
        private bool _isDraggingViewport;

        public NodeEditorViewModel ViewModel => DataContext as NodeEditorViewModel;

        public MiniMapControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += MiniMapControl_DataContextChanged;
        }

        private void MiniMapControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (ViewModel != null)
            {
                ViewModel.Nodes.CollectionChanged += (s, e) => UpdateMiniMap();
                ViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(NodeEditorViewModel.ViewportOffsetX) ||
                        e.PropertyName == nameof(NodeEditorViewModel.ViewportOffsetY) ||
                        e.PropertyName == nameof(NodeEditorViewModel.ViewportScale))
                    {
                        UpdateViewportRect();
                    }
                };
                UpdateMiniMap();
            }
        }

        private void UpdateMiniMap()
        {
            if (ViewModel == null) return;

            MiniMapCanvas.Children.Clear();

            // Draw nodes as small rectangles
            foreach (var node in ViewModel.Nodes)
            {
                var rect = new Rectangle
                {
                    Width = node.Width * MiniMapScale,
                    Height = node.Height * MiniMapScale,
                    Fill = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    Opacity = 0.7
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
            if (ViewModel == null) return;

            // Assume a viewport size (this should ideally come from the actual canvas size)
            double viewportWidth = 800;
            double viewportHeight = 600;

            ViewportRect.Width = (viewportWidth / ViewModel.ViewportScale) * MiniMapScale;
            ViewportRect.Height = (viewportHeight / ViewModel.ViewportScale) * MiniMapScale;

            Canvas.SetLeft(ViewportRect, -ViewModel.ViewportOffsetX * MiniMapScale);
            Canvas.SetTop(ViewportRect, -ViewModel.ViewportOffsetY * MiniMapScale);
        }

        private void ViewportRect_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDraggingViewport = true;
            ViewportRect.CapturePointer(e.Pointer);
            e.Handled = true;
        }

        private void ViewportRect_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDraggingViewport && ViewModel != null)
            {
                var position = e.GetCurrentPoint(MiniMapCanvas).Position;
                
                ViewModel.ViewportOffsetX = -(position.X - ViewportRect.Width / 2) / MiniMapScale;
                ViewModel.ViewportOffsetY = -(position.Y - ViewportRect.Height / 2) / MiniMapScale;

                e.Handled = true;
            }
        }

        private void ViewportRect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isDraggingViewport)
            {
                _isDraggingViewport = false;
                ViewportRect.ReleasePointerCapture(e.Pointer);
                e.Handled = true;
            }
        }
    }
}
