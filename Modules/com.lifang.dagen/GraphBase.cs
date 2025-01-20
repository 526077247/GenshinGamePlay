using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph
{
    [Serializable]
    public abstract class GraphBase: ScriptableObject
    {
        public Vector2 currentPanOffset = Vector2.zero;
        public float currentZoom = 1f;
        public int windowID;
        public string startNodeId;

        public bool leftInRightOut;
        public List<NodeBase> values = new();
        public List<Edge> edges = new();
        protected virtual T CreateNodeBase<T>() where T: NodeBase
        {
            var node = CreateInstance<T>() ;
            node.name = "Node";
            AssetDatabase.AddObjectToAsset(node,this);
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
            AssetDatabase.AddObjectToAsset(edge,this);
            return edge;
        }
        
        protected virtual void RemoveEdgeBase(Edge edge)
        {
            DestroyImmediate(edge,true);
        }
    }
}