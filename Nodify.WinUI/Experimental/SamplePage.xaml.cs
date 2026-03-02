using Microsoft.UI.Xaml.Controls;
using Nodify.WinUI.Experimental.ViewModel;
using Nodify.WinUI.Experimental.Helpers;
using Nodify.WinUI.Experimental.Model;
using Windows.Storage;
using System;
using Microsoft.UI.Xaml;

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

    private void OnAddNodeClick(object sender, RoutedEventArgs e)
    {
        NodeEditor?.AddNode();
    }

    private void OnZoomInClick(object sender, RoutedEventArgs e)
    {
        NodeEditor?.ZoomIn();
    }

    private void OnZoomOutClick(object sender, RoutedEventArgs e)
    {
        NodeEditor?.ZoomOut();
    }

    private void OnResetViewClick(object sender, RoutedEventArgs e)
    {
        NodeEditor?.ResetView();
    }

    private void OnResetZoomClick(object sender, RoutedEventArgs e)
    {
        if (NodeEditor?.ViewModel != null)
        {
            NodeEditor.ViewModel.ViewportScale = 1.0;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        StorageFile? file = await NodeEditor?.SaveAsync();
        if (file != null && NodeEditor?.ViewModel != null)
        {
            NodeEditor.ViewModel.CurrentFileName = file.Name;
        }
    }

    private async void OnLoadClick(object sender, RoutedEventArgs e)
    {
        StorageFile? file = await NodeEditor?.LoadAsync();
        if (file != null && NodeEditor?.ViewModel != null)
        {
            NodeEditor.ViewModel.CurrentFileName = file.Name;
        }
    }
}