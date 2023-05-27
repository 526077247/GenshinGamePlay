
using DaGenGraph;
using DaGenGraph.Editor;
using UnityEngine;

namespace TaoTie
{
    public class AINode:Node
    {
        public override NodeView GetNodeView()
        {
            return new AINodeView();
        }

        public override void InitNode(Graph graph, Vector2 pos, string _name, int minimumInputPortsCount = 1, int minimumOutputPortsCount = 0)
        {
            base.InitNode(graph, pos, _name, minimumInputPortsCount, minimumOutputPortsCount);
            SetName("AINode");
            
        }
    }
}