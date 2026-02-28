using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Nodify.WinUI.Experimental.ViewModel;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.View
{
    public sealed partial class PortControl : UserControl
    {
        public PortViewModel ViewModel => DataContext as PortViewModel;

        public event EventHandler<PortViewModel> ConnectionStarted;
        public event EventHandler<PortViewModel> ConnectionCompleted;

        public PortControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += PortControl_DataContextChanged;
            this.Loaded += PortControl_Loaded;
        }

        private void PortControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            UpdatePortPosition();
        }

        private void PortControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePortPosition();
        }

        private void UpdatePortPosition()
        {
            if (ViewModel == null) return;

            try
            {
                // Get the center position of the port in canvas coordinates
                var transform = this.TransformToVisual(GetCanvasParent());
                var position = transform.TransformPoint(new Point(8, 8)); // Center of the ellipse
                ViewModel.Position = position;
            }
            catch
            {
                // Ignore if transform fails
            }
        }

        private UIElement GetCanvasParent()
        {
            DependencyObject parent = this;
            while (parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is Canvas || parent is NodeEditorCanvas)
                    return parent as UIElement;
            }
            return null;
        }

        private void PortEllipse_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PortEllipse.Width = 20;
            PortEllipse.Height = 20;
        }

        private void PortEllipse_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            PortEllipse.Width = 16;
            PortEllipse.Height = 16;
        }

        private void PortEllipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UpdatePortPosition();
            ConnectionStarted?.Invoke(this, ViewModel);
            e.Handled = true;
        }

        private void PortEllipse_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            UpdatePortPosition();
            ConnectionCompleted?.Invoke(this, ViewModel);
            e.Handled = true;
        }

        public void UpdatePosition()
        {
            UpdatePortPosition();
        }
    }
}
