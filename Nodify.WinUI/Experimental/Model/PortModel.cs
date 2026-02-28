using System;
using System.Text.Json.Serialization;

namespace Nodify.WinUI.Experimental.Model
{
    /// <summary>
    /// Port direction enumeration
    /// </summary>
    public enum PortDirection
    {
        Input,
        Output
    }

    /// <summary>
    /// Port type enumeration (can be extended for type checking)
    /// </summary>
    public enum PortType
    {
        Default,
        Execution,
        Data,
        Event
    }

    /// <summary>
    /// Model representing a node port
    /// </summary>
    public class PortModel
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("direction")]
        public PortDirection Direction { get; set; }

        [JsonPropertyName("type")]
        public PortType Type { get; set; }

        [JsonPropertyName("nodeId")]
        public Guid NodeId { get; set; }

        public PortModel()
        {
            Id = Guid.NewGuid();
            Name = "Port";
            Type = PortType.Default;
        }

        public PortModel(string name, PortDirection direction, PortType type = PortType.Default)
        {
            Id = Guid.NewGuid();
            Name = name;
            Direction = direction;
            Type = type;
        }
    }
}
