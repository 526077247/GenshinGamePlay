﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace DaGenGraph.Editor
{
    public partial class GraphWindow : EditorWindow
    {
        public static GraphWindow instance;
        
        // [MenuItem("Tools/DaGenGraph")]
        public static void ShowAIGraph()
        {
            if (instance==null)
            {
                instance = CreateWindow<GraphWindow>();
                instance.titleContent = new GUIContent("DaGenGraph");
            }
            instance.Show();
        }

        protected Graph m_Graph;
        private GraphMode m_Mode = GraphMode.None;
        private float m_Timer;
        private float m_LastUpdateTime;
        private float m_AniTime;
        private bool m_AnimateInput;
        private bool m_AnimateOutput;
        private bool m_HasFocus;
        private Dictionary<string, NodeView> m_NodeViews;
        private Dictionary<string, List<VirtualPoint>> m_Points;
        private Dictionary<string, Port> m_Ports;
        private Dictionary<string, EdgeView> m_EdgeViews;

        private Dictionary<string, EdgeView> edgeViews
        {
            get
            {
                if (m_EdgeViews != null) return m_EdgeViews;
                m_EdgeViews = new Dictionary<string, EdgeView>();
                foreach (var port in ports.Values)
                {
                    for (int i = port.edges.Count - 1; i >= 0; i--)
                    {
                        var edge = port.edges[i];
                        if (edge == null)
                        {
                            port.edges.RemoveAt(i);
                            continue;
                        }

                        if (!nodeViews.ContainsKey(edge.inputNodeId) ||
                            !nodeViews.ContainsKey(edge.outputNodeId))
                        {
                            port.ContainsEdge(edge.id);
                            continue;
                        }

                        //check if the EdgeViews id has been added to the dictionary
                        if (!m_EdgeViews.ContainsKey(edge.id))
                            m_EdgeViews.Add(edge.id, new EdgeView { edgeId = edge.id });
                        if (edge.inputPortId == port.id)
                            m_EdgeViews[edge.id].inputPort =
                                port; //reference this Prot if it is the EdgeViews's InputProt
                        if (edge.inputNodeId == port.nodeId)
                            m_EdgeViews[edge.id].inputNode =
                                nodeViews[port.nodeId].node; //reference this Prot's parent as the InputNode
                        if (edge.outputPortId == port.id)
                            m_EdgeViews[edge.id].outputPort =
                                port; //reference this socket if it is the EdgeView's OutputProt
                        if (edge.outputNodeId == port.nodeId)
                            m_EdgeViews[edge.id].outputNode =
                                nodeViews[port.nodeId].node; //reference this Prot's parent as the OutputNode
                    }
                }

                return m_EdgeViews;
            }
        }

        private Dictionary<string, Port> ports
        {
            get
            {
                if (m_Ports != null)
                {
                    return m_Ports;
                }

                m_Ports = new Dictionary<string, Port>();
                foreach (var nodeView in nodeViews.Values)
                {
                    for (var i = nodeView.node.inputPorts.Count - 1; i >= 0; i--)
                    {
                        var inputSocket = nodeView.node.inputPorts[i];
                        if (inputSocket == null)
                        {
                            nodeView.node.inputPorts.RemoveAt(i);
                            EditorUtility.SetDirty(nodeView.node);
                            continue;
                        }

                        m_Ports.Add(inputSocket.id, inputSocket);
                    }

                    for (int i = nodeView.node.outputPorts.Count - 1; i >= 0; i--)
                    {
                        Port outputSocket = nodeView.node.outputPorts[i];
                        if (outputSocket == null)
                        {
                            nodeView.node.outputPorts.RemoveAt(i);
                            EditorUtility.SetDirty(nodeView.node);
                            continue;
                        }

                        m_Ports.Add(outputSocket.id, outputSocket);
                    }
                }

                return m_Ports;
            }
        }

        private Dictionary<string, List<VirtualPoint>> points
        {
            get
            {
                if (m_Points != null)
                {
                    return m_Points;
                }

                m_Points = new Dictionary<string, List<VirtualPoint>>();
                foreach (var nodeView in nodeViews.Values)
                {
                    foreach (var port in nodeView.node.inputPorts)
                    {
                        m_Points.Add(port.id, new List<VirtualPoint>());
                        foreach (var point in port.edgePoints)
                        {
                            m_Points[port.id].Add(new VirtualPoint(nodeViews[port.nodeId].node, port,
                                point + m_Graph.currentPanOffset / m_Graph.currentZoom, point));
                        }
                    }

                    foreach (var port in nodeView.node.outputPorts)
                    {
                        m_Points.Add(port.id, new List<VirtualPoint>());
                        foreach (var point in port.edgePoints)
                        {
                            m_Points[port.id].Add(new VirtualPoint(nodeViews[port.nodeId].node, port,
                                point + m_Graph.currentPanOffset / m_Graph.currentZoom, point));
                        }
                    }
                }

                return m_Points;
            }
        }

        private Dictionary<string, NodeView> nodeViews
        {
            get { return m_NodeViews; }
        }

        protected virtual void OnEnable()
        {
            m_AltKeyPressedAnimBool = new AnimBool(false, Repaint);
            Init(null);
        }

        protected void Init(Graph graph)
        {
            m_Graph = graph == null ? CreateInstance<Graph>() : graph;
            m_NodeViews = new Dictionary<string, NodeView>();
            var count = 0;
            foreach (var node in m_Graph.nodes.Values)
            {
                var nodeView = node.GetNodeView();
                nodeView.Init(count++, node, m_Graph);
                m_NodeViews.Add(nodeView.node.id, nodeView);
            }
        }

        private void OnGUI()
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (DragAndDrop.objectReferences.ToList().Exists(o => !(o is GameObject)))
                    {
                        return;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        OnGameObjectsDragIn(DragAndDrop.objectReferences.Cast<GameObject>().ToArray(),
                            evt.mousePosition);
                    }

                    break;
            }

            DrawViewGraph();
        }

        private void Update()
        {
            float curTime = Time.realtimeSinceStartup;
            m_LastUpdateTime = Mathf.Min(m_LastUpdateTime, curTime); //very important!!!
            m_Timer = curTime - m_LastUpdateTime;
            m_LastUpdateTime = curTime;
        }

        private void OnFocus()
        {
            m_HasFocus = true;
        }

        private void OnLostFocus()
        {
            m_HasFocus = false;
            m_SelectedNodes.Clear();
            UpdateNodesSelectedState(m_SelectedNodes);
        }

        private void DrawViewGraph()
        {
            ConstructGraphGUI();
            var graphArea = new Rect(0, 0, position.width, position.height);
            GraphBackground.DrawGrid(graphArea, m_Graph.currentZoom, Vector2.zero);
            m_Graph.graphAreaIncludingTab =
                new Rect(0, 20, position.width, position.height);
            m_Graph.scaledGraphArea = new Rect(0, 0,
                graphArea.width / m_Graph.currentZoom,
                graphArea.height / m_Graph.currentZoom);
            var initialMatrix = GUI.matrix; //save initial matrix
            HandleMouseHover();
            GUI.EndClip();
            GUI.BeginClip(new Rect(m_Graph.graphAreaIncludingTab.position, m_Graph.scaledGraphArea.size));
            var translation = Matrix4x4.TRS(m_Graph.graphAreaIncludingTab.position, Quaternion.identity, Vector3.one);
            var scale = Matrix4x4.Scale(Vector3.one * m_Graph.currentZoom);
            {
                GUI.matrix = translation * scale * translation.inverse;
                {
                    DrawEdges();
                    DrawNodes(graphArea);
                    DrawPortsEdgePoints();
                    DrawLineFromPortToPosition(m_ActivePort, Event.current.mousePosition);
                    DrawSelectionBox();
                }
            }
            GUI.EndClip();
            GUI.BeginClip(m_Graph.graphAreaIncludingTab);
            GUI.matrix = initialMatrix; //reset the matrix to the initial value
            DrawToolbar();
            HandleZoom();
            HandlePanning();
            HandleMouseRightClicks();
            HandleMouseMiddleClicks();
            HandleMouseLeftClicks();
            HandleKeys();
            WhileDraggingUpdateSelectedNodes();
        }

        private void WhileDraggingUpdateSelectedNodes()
        {
            if (m_Mode != GraphMode.Drag) return;

            var validatePointsDatabase = false; //bool that triggers a PointsDatabase validation
            var validateConnectionsDatabase = false; //bool that triggers a ConnectionsDatabase validation

            foreach (var selectedNode in m_SelectedNodes) //go through all the selected nodes
            {
                foreach (var virtualPoints in selectedNode.outputPorts.Select(outputSocket => points[outputSocket.id]))
                {
                    if (virtualPoints == null)
                    {
                        validatePointsDatabase = true;
                        continue;
                    }

                    foreach (var virtualPoint in virtualPoints)
                    {
                        if (virtualPoint == null) //point is null -> trigger validation
                        {
                            validatePointsDatabase = true;
                            continue;
                        }

                        if (virtualPoint.node == null) //point parent Node is null -> trigger validation
                        {
                            validatePointsDatabase = true;
                            continue;
                        }

                        virtualPoint
                            .CalculateRect(); //recalculate the rect to reflect the new values (the new WorldPosition)
                    }
                }

                foreach (Port inputSocket in selectedNode.inputPorts) //get the node's input sockets
                {
                    List<VirtualPoint> virtualPoints = points[inputSocket.id]; //get input socket's virtual points list
                    if (virtualPoints == null) //list is null -> trigger validation
                    {
                        validatePointsDatabase = true;
                        continue;
                    }

                    foreach (VirtualPoint virtualPoint in virtualPoints)
                    {
                        if (virtualPoint == null) //point is null -> trigger validation
                        {
                            validatePointsDatabase = true;
                            continue;
                        }

                        if (virtualPoint.node == null) //point parent Node is null -> trigger validation
                        {
                            validatePointsDatabase = true;
                            continue;
                        }

                        virtualPoint
                            .CalculateRect(); //recalculate the rect to reflect the new values (the new WorldPosition)
                    }
                }

                foreach (var ev in edgeViews.Values) //get all the virtual connections in the graph
                {
                    //check the virtual connections for nulls -> if any null is found -> trigger validation
                    if (ev == null ||
                        ev.outputNode == null ||
                        ev.outputPort == null ||
                        ev.outputVirtualPoint == null ||
                        ev.inputNode == null ||
                        ev.inputPort == null ||
                        ev.inputVirtualPoint == null)
                    {
                        validateConnectionsDatabase = true;
                        continue;
                    }

                    if (ev.inputNode.id != selectedNode.id && ev.outputNode.id != selectedNode.id) continue;
                    CalculateConnectionCurve(
                        ev); //recalculate the connection curve to reflect the new values (the new WorldPosition)
                }
            }

            if (validatePointsDatabase)
                ValidatePointsDatabase();
            if (validateConnectionsDatabase)
                ValidateConnectionsDatabase();
        }

        private void ValidatePointsDatabase()
        {
            if (points == null) return;
            var invalidKeys = new List<string>(); //temp keys list
            foreach (string key in points.Keys)
            {
                if (points[key] == null)
                {
                    invalidKeys.Add(key); //null virtual points list -> remove key
                    continue;
                }

                var vList = points[key];
                var foundInvalidVirtualPoint = false;
                foreach (var virtualPoint in vList)
                {
                    if (virtualPoint == null)
                    {
                        foundInvalidVirtualPoint = true; //null virtual point -> mark virtual point as invalid
                        break;
                    }

                    if (virtualPoint.node == null)
                    {
                        foundInvalidVirtualPoint =
                            true; //null virtual point parent Node -> mark virtual point as invalid
                        break;
                    }

                    if (virtualPoint.port == null)
                    {
                        foundInvalidVirtualPoint =
                            true; //null virtual point parent Socket -> mark virtual point as invalid
                        break;
                    }
                }

                if (foundInvalidVirtualPoint)
                    invalidKeys.Add(key); //found an invalid virtual point in the virtual points list -> remove key
            }

            foreach (string invalidKey in invalidKeys)
            {
                points.Remove(invalidKey); //remove invalid keys
            }
        }

        private void ValidateConnectionsDatabase()
        {
            if (m_EdgeViews == null) return;
            var invalidKeys = new List<string>(); //temp keys list
            foreach (var key in edgeViews.Keys)
            {
                if (edgeViews[key] == null)
                {
                    invalidKeys.Add(key); //null virtual connection -> remove key
                    continue;
                }

                var edgeView = edgeViews[key];
                if (edgeView.outputNode == null)
                {
                    invalidKeys.Add(key); //null output node -> remove key
                    continue;
                }

                if (edgeView.inputNode == null)
                {
                    invalidKeys.Add(key); //null input node -> remove key
                    continue;
                }

                if (edgeView.outputPort == null)
                {
                    invalidKeys.Add(key); //null output socket -> remove key
                    continue;
                }

                if (edgeView.inputPort == null) invalidKeys.Add(key); //null input socket -> remove key
            }

            foreach (string invalidKey in invalidKeys)
            {
                edgeViews.Remove(invalidKey); //remove invalid keys
            }
        }

        private void ConstructGraphGUI()
        {
            if (m_Graph == null) return;
            m_Points = null;
            m_Ports = null;
            m_EdgeViews = null;
            m_Points = points;
            m_Ports = ports;
            m_EdgeViews = edgeViews;
            //calculate all the connection points initial values
            CalculateAllPointRects();
            //calculate all the connections initial values
            //we do this here because in normal operation we want to update only the connections that are referencing nodes that are being dragged
            CalculateAllConnectionCurves();
            //update the visual state of all the connection points
            UpdateVirtualPointsIsOccupiedStates();
            //check for errors
            CheckAllNodesForErrors();
        }

        private void CalculateAllPointRects()
        {
            foreach (string key in points.Keys)
            foreach (VirtualPoint virtualPoint in points[key])
                virtualPoint.CalculateRect();
        }

        private void CalculateAllConnectionCurves()
        {
            foreach (EdgeView edgeView in m_EdgeViews.Values)
            {
                if (edgeView == null ||
                    edgeView.outputNode == null ||
                    edgeView.outputPort == null ||
                    edgeView.inputNode == null ||
                    edgeView.inputPort == null)
                    continue;

                CalculateConnectionCurve(edgeView);
            }
        }

        private void CalculateConnectionCurve(EdgeView ev)
        {
            //get the lists of all the calculated virtual points for both Ports
            var outputVirtualPoints = points[ev.outputPort.id];
            var inputVirtualPoints = points[ev.inputPort.id];

            //get both OutputPort and InputPort rects converted to WorldSpace
            var outputPortWorldRect = GetPortWorldRect(ev.outputPort);
            var inputPortWorldRect = GetPortWorldRect(ev.inputPort);

            //get position values needed to determine the connection points and curve settings
            var outputPortCenter = outputPortWorldRect.center.x;
            var inputPortCenter = inputPortWorldRect.center.x;

            //get the closest virtual points for both Ports
            float minDistance = 100000;
            if (m_Graph.leftInRightOut)
            {
                ev.outputVirtualPoint = outputVirtualPoints[1];
                ev.inputVirtualPoint = inputVirtualPoints[0];
                minDistance = Vector2.Distance(ev.outputVirtualPoint.rect.position, ev.inputVirtualPoint.rect.position);
            }
            else
            {
                foreach (var outputVirtualPoint in outputVirtualPoints)
                {
                    foreach (var inputVirtualPoint in inputVirtualPoints)
                    {
                        var currentDistance = Vector2.Distance(outputVirtualPoint.rect.position,
                            inputVirtualPoint.rect.position);
                        if (currentDistance > minDistance) continue;
                        ev.outputVirtualPoint = outputVirtualPoint;
                        ev.inputVirtualPoint = inputVirtualPoint;
                        minDistance = currentDistance;
                    }
                }
            }

            //set both the output and the input points as their respective tangents
            var zoomedPanOffset = m_Graph.currentPanOffset / m_Graph.currentZoom;

            var outputPoint = ev.outputVirtualPoint.rect.position - zoomedPanOffset;
            var inputPoint = ev.inputVirtualPoint.rect.position - zoomedPanOffset;
            var outputNodeWidth = ev.outputNode.GetWidth();
            var inputNodeWidth = ev.inputNode.GetWidth();
            var widthDifference = outputNodeWidth > inputNodeWidth
                ? outputNodeWidth - inputNodeWidth
                : inputNodeWidth - outputNodeWidth;

            ev.outputTangent = outputPoint + zoomedPanOffset;
            ev.inputTangent = inputPoint + zoomedPanOffset;

            //UP TO THIS POINT WE HAVE A STRAIGHT LINE
            //from here we start calculating the custom tangent values -> thus turning our connection line into a dynamic curve

            Vector2 outputTangentArcDirection; //output point tangent
            Vector2 inputTangentArcDirection; //input point tangent

            //OUTPUT RIGHT CONNECTION
            if (outputPortCenter < inputPortCenter && outputPortCenter <= inputPoint.x ||
                outputPortCenter >= inputPortCenter && outputPoint.x >= inputPortCenter &&
                outputPortCenter <= inputPoint.x)
            {
                if (outputPoint.x <= inputPortCenter + widthDifference / 2 && inputPortCenter > outputPoint.x)
                {
                    outputTangentArcDirection = Vector2.right;
                    inputTangentArcDirection = Vector2.left;
                }
                else
                {
                    outputTangentArcDirection = Vector2.right;
                    inputTangentArcDirection = Vector2.right;
                }
            }
            //OUTPUT LEFT CONNECTION
            else
            {
                if (outputPoint.x >= inputPortCenter)
                {
                    outputTangentArcDirection = Vector2.left;
                    inputTangentArcDirection = Vector2.right;
                }
                else
                {
                    outputTangentArcDirection = Vector2.left;
                    inputTangentArcDirection = Vector2.left;
                }
            }

            //set the curve strength (curvature) to be dynamic, by taking into account the distance between the connection points
            var outputCurveStrength = minDistance * (0.48f + ev.outputPort.curveModifier);
            var inputCurveStrength = minDistance * (0.48f + ev.inputPort.curveModifier);
            //update the tangents with the dynamic values
            ev.outputTangent += outputTangentArcDirection * outputCurveStrength;
            ev.inputTangent += inputTangentArcDirection * inputCurveStrength;
        }

        private Rect GetPortWorldRect(Port port)
        {
            if (port == null) return new Rect();
            var parentNode = nodeViews[port.nodeId].node;
            if (parentNode == null) return new Rect();
            var socketWorldRect = new Rect(parentNode.GetX(),
                parentNode.GetY() + port.GetY(),
                port.GetWidth(),
                port.GetHeight());
            return socketWorldRect;
        }

        private void CheckAllNodesForErrors()
        {
            foreach (var nodeView in nodeViews.Values)
            {
                nodeView.node.CheckForErrors();
            }

            Repaint();
        }

        private enum GraphMode
        {
            None,
            Connect,
            Select,
            Drag,
            Pan,
            Delete
        }

        private enum GraphAction
        {
            Copy,
            Connect,
            CreateNode,
            DeleteNodes,
            DeselectAll,
            Disconnect,
            Paste,
            SelectAll,
            SelectNodes
        }

        private class SelectPoint
        {
            public readonly float x;
            public readonly float y;

            public SelectPoint(Vector2 position)
            {
                x = position.x;
                y = position.y;
            }
        }
    }
}