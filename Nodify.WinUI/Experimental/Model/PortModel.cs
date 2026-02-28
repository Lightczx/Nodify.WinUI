using System;
using System.Text.Json.Serialization;

namespace Nodify.WinUI.Experimental.Model;

/// <summary>
/// Model representing a node port
/// </summary>
public class PortModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("direction")]
    public PortDirection Direction { get; set; }

    [JsonPropertyName("type")]
    public PortType Type { get; set; }

    [JsonPropertyName("nodeId")]
    public Guid NodeId { get; set; }

    public static PortModel Create()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = "Port",
            Type = PortType.Default,
        };
    }

    public static PortModel Create(string name, PortDirection direction, Guid nodeId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Direction = direction,
            Type = PortType.Default,
            NodeId = nodeId,
        };
    }

    public static PortModel Create(string name, PortDirection direction, PortType type, Guid nodeId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Direction = direction,
            Type = type,
            NodeId = nodeId,
        };
    }
}