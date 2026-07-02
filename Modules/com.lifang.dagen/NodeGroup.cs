using System;
using System.Collections.Generic;
using UnityEngine;

namespace DaGenGraph
{
    [Serializable]
    public class NodeGroup
    {
        public string id;
        public string title = "Node Group";
        public List<string> nodeIds = new List<string>();

        // Position when collapsed
        public float x;
        public float y;
        public float collapsedWidth = 220f;
        public float collapsedHeight = 70f;

        public bool isCollapsed;

        [NonSerialized] public bool isHovered;
        [NonSerialized] public bool isTitleEditing;
        [NonSerialized] public string titleEditBuffer;

        // Runtime: maps portId -> screen position on collapsed box (computed each frame)
        [NonSerialized] public Dictionary<string, Vector2> collapsedPortScreenPos = new();
        // Runtime: maps portId -> the Port object (for external-facing ports)
        [NonSerialized] public Dictionary<string, Port> collapsedPorts = new();

        // Persisted renames for collapsed port labels: portId -> customLabel
        public Dictionary<string, string> collapsedPortLabels = new();

        // Padding around member nodes when expanded
        public const float k_Padding = 30f;
        public const float k_TitleBarHeight = 28f;

        public void GenerateNewId()
        {
            id = Guid.NewGuid().ToString();
        }

        public Rect GetCollapsedRect()
        {
            return new Rect(x, y, collapsedWidth, collapsedHeight);
        }

        public Rect GetExpandedRect(List<NodeBase> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return new Rect(x, y, collapsedWidth, collapsedHeight);

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var node in nodes)
            {
                if (node == null) continue;
                minX = Mathf.Min(minX, node.GetX());
                minY = Mathf.Min(minY, node.GetY());
                maxX = Mathf.Max(maxX, node.GetX() + node.GetWidth());
                maxY = Mathf.Max(maxY, node.GetY() + node.GetHeight());
            }
            return new Rect(
                minX - k_Padding,
                minY - k_Padding - k_TitleBarHeight,
                maxX - minX + k_Padding * 2,
                maxY - minY + k_Padding * 2 + k_TitleBarHeight
            );
        }

        public Rect GetCurrentRect(List<NodeBase> nodes)
        {
            return isCollapsed ? GetCollapsedRect() : GetExpandedRect(nodes);
        }

        /// <summary> Get the collapsed box height needed to fit all external ports </summary>
        public float GetCollapsedHeightWithPorts(int externalPortCount)
        {
            float h = k_TitleBarHeight + 16f; // title + subtitle
            h += externalPortCount * 22f; // each port row
            return Mathf.Max(h, collapsedHeight);
        }
    }
}
