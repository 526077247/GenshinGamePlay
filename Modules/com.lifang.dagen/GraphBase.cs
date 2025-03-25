using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DaGenGraph
{
    [Serializable]
    public abstract class GraphBase: ScriptableObject
    {
        [HideInInspector]
        public bool showNodeViewDetails = true;
        [HideInInspector]
        public Vector2 currentPanOffset = Vector2.zero;
        [HideInInspector]
        public float currentZoom = 1f;
        [HideInInspector]
        public int windowID;
        [HideInInspector]
        public string startNodeId;
        [HideInInspector]
        public bool leftInRightOut;
        [HideInInspector]
        public List<NodeBase> values = new();
        [HideInInspector]
        public List<Edge> edges = new();
        protected virtual T CreateNodeBase<T>() where T: NodeBase
        {
            var node = CreateInstance<T>() ;
            node.name = "Node";
#if UNITY_EDITOR
            try
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
#endif
            return node;
        }
        
        protected virtual void DeleteNodeBase<T>(T nodeBase) where T: NodeBase
        {
            DestroyImmediate(nodeBase,true);
        }

        public NodeBase GetStartNode()
        {
            return FindNode(startNodeId);
        }

        public NodeBase FindNode(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i].id == nodeId)
                    {
                        return values[i];
                    }
                }
            }

            return null;
        }
        
        public T CreateNode<T>(Vector2 pos, string nodeName = "Node", bool isRoot = false) where T: NodeBase
        {
            var node = CreateNodeBase<T>();
            node.InitNode(WorldToGridPosition(pos), nodeName);
            values.Add(node);
            if ((isRoot || string.IsNullOrEmpty(startNodeId)) && values.Count>0)
            {
                startNodeId = values.First().id;
            }
            node.AddDefaultPorts();
            return node;
        }

        public void RemoveNode(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i].id == nodeId)
                    {
                        DeleteNodeBase(values[i]);
                        values.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        
        public Vector2 WorldToGridPosition(Vector2 worldPosition)
        {
            return (worldPosition - currentPanOffset) / currentZoom;
        }
        
        public Edge GetEdge(string edgeId)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].id == edgeId)
                {
                    return edges[i];
                }
            }
            return null;
        }
        
        public Edge CreateEdge(Port outputPort, Port inputPort)
        {
            var edge = CreateEdgeBase();
            edge.Init(outputPort, inputPort);
            edges.Add(edge);
            outputPort.edges.Add(edge.id);
            inputPort.edges.Add(edge.id);
            return edge;
        }

        public void RemoveEdge(string edgeId)
        {
            if (!string.IsNullOrEmpty(edgeId))
            {
                for (int i = 0; i < edges.Count; i++)
                {
                    if (edges[i].id == edgeId)
                    {
                        var edge = edges[i];
                        edges.RemoveAt(i);
                        DestroyImmediate(edge, true);
                        break;
                    }
                }
            }
            
        }

        protected virtual Edge CreateEdgeBase()
        {
            var edge = CreateInstance<Edge>() ;
            edge.name = "Edge";
#if UNITY_EDITOR
            try
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(edge, this);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
#endif
            return edge;
        }
        
        protected virtual void RemoveEdgeBase(Edge edge)
        {
            DestroyImmediate(edge,true);
        }
    }
}