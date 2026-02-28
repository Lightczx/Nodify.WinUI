using System;
using System.Text.Json.Serialization;

namespace Nodify.WinUI.Experimental.Model;

/// <summary>
/// Model representing a connection between two ports
/// </summary>
public class ConnectionModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("sourcePortId")]
    public Guid SourcePortId { get; set; }

    [JsonPropertyName("targetPortId")]
    public Guid TargetPortId { get; set; }

    [JsonPropertyName("sourceNodeId")]
    public Guid SourceNodeId { get; set; }

    [JsonPropertyName("targetNodeId")]
    public Guid TargetNodeId { get; set; }

    public static ConnectionModel Create()
    {
        return new()
        {
            Id = Guid.NewGuid(),
        };
    }

    public static ConnectionModel Create(Guid sourcePortId, Guid targetPortId, Guid sourceNodeId, Guid targetNodeId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            SourcePortId = sourcePortId,
            TargetPortId = targetPortId,
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
        };
    }
}