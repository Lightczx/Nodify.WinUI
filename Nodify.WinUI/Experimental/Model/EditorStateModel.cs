using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nodify.WinUI.Experimental.Model
{
    /// <summary>
    /// Model representing the entire editor state for serialization
    /// </summary>
    public class EditorStateModel
    {
        [JsonPropertyName("nodes")]
        public List<NodeModel> Nodes { get; set; }

        [JsonPropertyName("connections")]
        public List<ConnectionModel> Connections { get; set; }

        [JsonPropertyName("viewportOffsetX")]
        public double ViewportOffsetX { get; set; }

        [JsonPropertyName("viewportOffsetY")]
        public double ViewportOffsetY { get; set; }

        [JsonPropertyName("viewportScale")]
        public double ViewportScale { get; set; }

        public EditorStateModel()
        {
            Nodes = new List<NodeModel>();
            Connections = new List<ConnectionModel>();
            ViewportOffsetX = 0;
            ViewportOffsetY = 0;
            ViewportScale = 1.0;
        }
    }
}
