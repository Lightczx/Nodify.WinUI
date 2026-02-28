using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nodify.WinUI.Experimental.Model;

/// <summary>
/// Model representing the entire editor state for serialization
/// </summary>
public class EditorStateModel
{
    [JsonPropertyName("nodes")]
    public List<NodeModel> Nodes { get; set; } = default!;

    [JsonPropertyName("connections")]
    public List<ConnectionModel> Connections { get; set; } = default!;

    [JsonPropertyName("viewportOffsetX")]
    public double ViewportOffsetX { get; set; }

    [JsonPropertyName("viewportOffsetY")]
    public double ViewportOffsetY { get; set; }

    [JsonPropertyName("viewportScale")]
    public double ViewportScale { get; set; }

    public static EditorStateModel Create()
    {
        return new()
        {
            Nodes = [],
            Connections = [],
            ViewportOffsetX = 0,
            ViewportOffsetY = 0,
            ViewportScale = 1.0,
        };
    }

    public static EditorStateModel Create(List<NodeModel> nodes, List<ConnectionModel> connections, double viewportOffsetX, double viewportOffsetY, double viewportScale)
    {
        return new()
        {
            Nodes = nodes,
            Connections = connections,
            ViewportOffsetX = viewportOffsetX,
            ViewportOffsetY = viewportOffsetY,
            ViewportScale = viewportScale,
        };
    }
}