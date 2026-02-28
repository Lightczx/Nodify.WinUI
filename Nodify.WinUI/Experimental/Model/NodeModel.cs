using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Windows.Foundation;

namespace Nodify.WinUI.Experimental.Model
{
    /// <summary>
    /// Model representing a node in the editor
    /// </summary>
    public class NodeModel
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("positionX")]
        public double PositionX { get; set; }

        [JsonPropertyName("positionY")]
        public double PositionY { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("inputPorts")]
        public List<PortModel> InputPorts { get; set; }

        [JsonPropertyName("outputPorts")]
        public List<PortModel> OutputPorts { get; set; }

        [JsonIgnore]
        public Point Position
        {
            get => new Point(PositionX, PositionY);
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }

        public NodeModel()
        {
            Id = Guid.NewGuid();
            Title = "New Node";
            Content = "";
            Width = 200;
            Height = 150;
            InputPorts = new List<PortModel>();
            OutputPorts = new List<PortModel>();
        }

        public NodeModel(string title, double x, double y)
        {
            Id = Guid.NewGuid();
            Title = title;
            Content = "";
            PositionX = x;
            PositionY = y;
            Width = 200;
            Height = 150;
            InputPorts = new List<PortModel>();
            OutputPorts = new List<PortModel>();
        }
    }
}
