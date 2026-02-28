using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Nodify.WinUI.Experimental.Model;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Nodify.WinUI.Experimental.Helpers;

/// <summary>
/// Helper class for serializing and deserializing the editor state
/// </summary>
public static class SerializationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(EditorStateModel state)
    {
        return JsonSerializer.Serialize(state, JsonOptions);
    }

    public static EditorStateModel? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<EditorStateModel>(json, JsonOptions);
    }

    public static async Task<bool> SaveToFileAsync(EditorStateModel state, StorageFile file)
    {
        try
        {
            string json = Serialize(state);
            await FileIO.WriteTextAsync(file, json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task<EditorStateModel?> LoadFromFileAsync(StorageFile file)
    {
        try
        {
            string? json = await FileIO.ReadTextAsync(file);
            return Deserialize(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<StorageFile?> PickSaveFileAsync(IntPtr windowHandle)
    {
        FileSavePicker savePicker = new();

        // Initialize with window handle for WinUI 3
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);

        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Node Graph", [".nodegraph"]);
        savePicker.SuggestedFileName = "NodeGraph";

        return await savePicker.PickSaveFileAsync();
    }

    public static async Task<StorageFile?> PickLoadFileAsync(IntPtr windowHandle)
    {
        FileOpenPicker openPicker = new();

        // Initialize with window handle for WinUI 3
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, windowHandle);

        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add(".nodegraph");

        return await openPicker.PickSingleFileAsync();
    }
}