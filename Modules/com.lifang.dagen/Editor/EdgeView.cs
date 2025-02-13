using UnityEngine;

namespace DaGenGraph.Editor
{
    public class EdgeView
    {
        /// <summary> The connection id. Every connection will have a record on two sockets (the output socket and the input socket) </summary>
        public string edgeId;
        /// <summary> Reference to the OutputPort Node of this connections </summary>
        public NodeBase outputNode;
        /// <summary> Reference to the OutputPort Port of the OutputPort Node of this connection </summary>
        public Port outputPort;
        /// <summary> Reference to the InputPort Node of this connection </summary>
        public NodeBase inputNode;
        /// <summary> Reference to the InputPort Port of the InputPort Node of this connection </summary>
        public Port inputPort;
        /// <summary> Reference to the OutputPort Virtual Point of this connection </summary>
        public VirtualPoint outputVirtualPoint;
        /// <summary> Reference to the InputPort Virtual Point of this connection </summary>
        public VirtualPoint inputVirtualPoint;
        /// <summary> Holds the last calculated value of the OutputPort Tangent in order to draw the connection curve (huge performance boost as we won't need to recalculate it on every frame) </summary>
        public Vector2 outputTangent = Vector2.zero;
        /// <summary> Holds the last calculated value of the InputPort Tangent in order to draw the connection curve (huge performance boost as we won't need to recalculate it on every frame) </summary>
        public Vector2 inputTangent = Vector2.zero;
        // public AnimBool ping = new AnimBool {speed = 0.6f};
        //TODO Edge Shader
        // /// <summary> Lightweight handles material </summary>
        // private static Material s_HandleMaterial;
        //
        // /// <summary> Returns a lightweight handles material </summary>
        // private static Material handleMaterial
        // {
        //     get
        //     {
        //         if (s_HandleMaterial != null) return s_HandleMaterial;
        //         Shader shader = Shader.Find("Hidden/Nody/LineDraw");
        //         var m = new Material(shader) {hideFlags = HideFlags.HideAndDontSave};
        //         s_HandleMaterial = m;
        //         return s_HandleMaterial;
        //     }
        // }
    }
}