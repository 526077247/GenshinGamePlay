using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DaGenGraph.Editor
{
    public abstract partial class GraphWindow  : DrawBase  
    {
        protected GraphBase m_Graph;
        private GraphMode m_Mode = GraphMode.None;
        private float m_Timer;
        private float m_LastUpdateTime;
        private float m_AniTime;
        private bool m_AnimateInput;
        private bool m_AnimateOutput;
        private bool m_HasFocus;
        private float m_CurrentZoom = 1f;
        private Rect m_GraphAreaIncludingTab;
        private Rect m_ScaledGraphArea;
        private bool m_DrawInspector = true;
        private Dictionary<string, NodeView> m_NodeViews;
        private Dictionary<string, List<VirtualPoint>> m_Points;
        private Dictionary<string, Port> m_Ports;
        private Dictionary<string, EdgeView> m_EdgeViews;
        private float m_LastZoom = 1f;
        private Vector2 m_LastPanOffset = Vector2.zero;
        private bool m_ForceRebuild;
        private bool m_Dirty = true;
        private readonly List<string> m_InvalidKeys = new();

        private float m_NodeInspectorWidth = 400;
        private float currentZoom
        {
            get
            {
                if (m_Graph != null)
                {
                    m_CurrentZoom = m_Graph.currentZoom;
                }

                return m_CurrentZoom;
            }
            set
            {
                m_CurrentZoom = value;
                if (m_Graph != null)
                {
                    m_Graph.currentZoom = m_CurrentZoom;
                }
            }
        }

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
                        var edgeId = port.edges[i];
                        var edge = m_Graph.GetEdge(edgeId);
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
                    if (nodeView.node.inputPorts != null)
                    {
                        foreach (var port in nodeView.node.inputPorts)
                        {
                            if(port==null) continue;
                            m_Points.Add(port.id, new List<VirtualPoint>());
                            foreach (var point in port.GetEdgePoints())
                            {
                                m_Points[port.id].Add(new VirtualPoint(nodeViews[port.nodeId].node, port,
                                    point + m_Graph.currentPanOffset / currentZoom, point));
                            }
                        }
                    }

                    if (nodeView.node.outputPorts != null)
                    {
                        foreach (var port in nodeView.node.outputPorts)
                        {
                            if(port==null) continue;
                            m_Points.Add(port.id, new List<VirtualPoint>());
                            foreach (var point in port.GetEdgePoints())
                            {
                                m_Points[port.id].Add(new VirtualPoint(nodeViews[port.nodeId].node, port,
                                    point + m_Graph.currentPanOffset / currentZoom, point));
                            }
                        }
                    }
                }

                return m_Points;
            }
        }

        private Dictionary<string, NodeView> nodeViews => m_NodeViews;

        protected virtual void OnEnable()
        {
            m_AltKeyPressedAnimBool = new AnimBool(false, Repaint);
            m_NodeViews = new Dictionary<string, NodeView>();
            AddButton(new GUIContent("新建"), InitGraph);
            AddButton(new GUIContent("打开"), LoadGraph);
            AddButton(new GUIContent("保存"), SaveGraph);
            AddButton(new GUIContent("展示或隐藏节点信息"), ChangeShowNodeViewDetails);
            AddButton(new GUIContent("详情面板"), ChangeDrawInspector, false);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                OnEnterPlayMode();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                OnExitPlayMode();
            }
            Repaint();
        }

        protected virtual void OnEnterPlayMode()
        {
            m_Graph = null;
            m_NodeViews?.Clear();
            m_Points = null;
            m_Ports = null;
            m_EdgeViews = null;
        }

        protected virtual void OnExitPlayMode() { }

        private void OnGUI()
        {
            // Lock the editor during Play mode
            if (EditorApplication.isPlaying)
            {
                DrawPlayModeLocked();
                return;
            }

            // No graph loaded
            if (m_Graph == null)
            {
                DrawEmptyState();
                return;
            }

            var evt = Event.current;
            if (evt.type is EventType.DragUpdated or EventType.DragPerform)
            {
                var refs = DragAndDrop.objectReferences;
                bool allGameObjects = true;
                for (int i = 0; i < refs.Length; i++)
                {
                    if (!(refs[i] is GameObject))
                    {
                        allGameObjects = false;
                        break;
                    }
                }
                if (!allGameObjects) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    var gameObjects = new GameObject[refs.Length];
                    for (int i = 0; i < refs.Length; i++)
                        gameObjects[i] = (GameObject)refs[i];
                    OnGameObjectsDragIn(gameObjects, evt.mousePosition);
                }
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

        private void DrawPlayModeLocked()
        {
            DrawToolbar();
            var area = new Rect(0, 20, position.width, position.height - 20);
            GraphBackground.DrawGrid(area, 1f, Vector2.zero);
            DrawCenteredMessage(area, "运行模式 — 编辑已锁定");
        }

        private void DrawEmptyState()
        {
            DrawToolbar();
            var area = new Rect(0, 20, position.width, position.height - 20);
            GraphBackground.DrawGrid(area, 1f, Vector2.zero);
            DrawCenteredMessage(area, "未加载图数据\n点击工具栏「新建」或「打开」");
        }

        private void DrawCenteredMessage(Rect area, string msg)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            var content = new GUIContent(msg);
            var size = style.CalcSize(content);
            var rect = new Rect(
                area.center.x - size.x / 2 - 20,
                area.center.y - size.y / 2 - 10,
                size.x + 40,
                size.y + 20);

            var oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(rect, content, style);
            GUI.color = oldColor;
        }

        private void OnFocus()
        {
            m_HasFocus = true;
        }

        private void OnLostFocus()
        {
            m_HasFocus = false;
            // m_SelectedNodes.Clear();
            // UpdateNodesSelectedState(m_SelectedNodes);
        }

        private void ChangeDrawInspector()
        {
            m_DrawInspector = !m_DrawInspector;
        }

        private void ChangeShowNodeViewDetails()
        {
            m_Graph.showNodeViewDetails = !m_Graph.showNodeViewDetails;
        }

        private void DrawViewGraph()
        {
            float nodeInspectorWidth = m_DrawInspector?m_NodeInspectorWidth:0;
            ConstructGraphGUI();
            var graphViewArea = new Rect(0, 0, position.width - nodeInspectorWidth, position.height);
            GraphBackground.DrawGrid(graphViewArea, currentZoom, Vector2.zero);
            if(m_DrawInspector) DrawInspector(graphViewArea.width, nodeInspectorWidth);
            m_GraphAreaIncludingTab = new Rect(0, 20, position.width, position.height);
            m_ScaledGraphArea = new Rect(0, 0, graphViewArea.width / currentZoom, graphViewArea.height / currentZoom);
            var initialMatrix = GUI.matrix; //save initial matrix
            HandleMouseHover();
            GUI.EndClip();
            GUI.BeginClip(new Rect(m_GraphAreaIncludingTab.position, m_ScaledGraphArea.size));
            var translation = Matrix4x4.TRS(m_GraphAreaIncludingTab.position, Quaternion.identity, Vector3.one);
            var scale = Matrix4x4.Scale(Vector3.one * currentZoom);
            {
                GUI.matrix = translation * scale * translation.inverse;
                {
                    DrawEdges();
                    DrawNodes(graphViewArea);
                    DrawPortsEdgePoints();
                    DrawLineFromPortToPosition(m_ActivePort, Event.current.mousePosition);
                    DrawSelectionBox();
                }
            }
            GUI.EndClip();
            GUI.BeginClip(m_GraphAreaIncludingTab);
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
                for (int i = 0; i < selectedNode.outputPorts.Count; i++)
                {
                    var outputSocket = selectedNode.outputPorts[i];
                    if (!points.TryGetValue(outputSocket.id, out var virtualPoints) || virtualPoints == null)
                    {
                        validatePointsDatabase = true;
                        continue;
                    }

                    for (int j = 0; j < virtualPoints.Count; j++)
                    {
                        var virtualPoint = virtualPoints[j];
                        if (virtualPoint == null || virtualPoint.node == null)
                        {
                            validatePointsDatabase = true;
                            continue;
                        }
                        virtualPoint.CalculateRect();
                    }
                }

                for (int i = 0; i < selectedNode.inputPorts.Count; i++)
                {
                    Port inputSocket = selectedNode.inputPorts[i];
                    if (!points.TryGetValue(inputSocket.id, out var virtualPoints) || virtualPoints == null)
                    {
                        validatePointsDatabase = true;
                        continue;
                    }

                    for (int j = 0; j < virtualPoints.Count; j++)
                    {
                        var virtualPoint = virtualPoints[j];
                        if (virtualPoint == null || virtualPoint.node == null)
                        {
                            validatePointsDatabase = true;
                            continue;
                        }
                        virtualPoint.CalculateRect();
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
            m_InvalidKeys.Clear();
            foreach (string key in points.Keys)
            {
                if (points[key] == null)
                {
                    m_InvalidKeys.Add(key);
                    continue;
                }

                var vList = points[key];
                var foundInvalidVirtualPoint = false;
                foreach (var virtualPoint in vList)
                {
                    if (virtualPoint == null)
                    {
                        foundInvalidVirtualPoint = true;
                        break;
                    }

                    if (virtualPoint.node == null)
                    {
                        foundInvalidVirtualPoint = true;
                        break;
                    }

                    if (virtualPoint.port == null)
                    {
                        foundInvalidVirtualPoint = true;
                        break;
                    }
                }

                if (foundInvalidVirtualPoint)
                    m_InvalidKeys.Add(key);
            }

            foreach (string invalidKey in m_InvalidKeys)
            {
                points.Remove(invalidKey);
            }
            m_InvalidKeys.Clear();
        }

        private void ValidateConnectionsDatabase()
        {
            if (m_EdgeViews == null) return;
            m_InvalidKeys.Clear();
            foreach (var key in edgeViews.Keys)
            {
                if (edgeViews[key] == null)
                {
                    m_InvalidKeys.Add(key);
                    continue;
                }

                var edgeView = edgeViews[key];
                if (edgeView.outputNode == null)
                {
                    m_InvalidKeys.Add(key);
                    continue;
                }

                if (edgeView.inputNode == null)
                {
                    m_InvalidKeys.Add(key);
                    continue;
                }

                if (edgeView.outputPort == null)
                {
                    m_InvalidKeys.Add(key);
                    continue;
                }

                if (edgeView.inputPort == null) m_InvalidKeys.Add(key);
            }

            foreach (string invalidKey in m_InvalidKeys)
            {
                edgeViews.Remove(invalidKey);
            }
            m_InvalidKeys.Clear();
        }

        private void ConstructGraphGUI()
        {
            if (m_Graph == null) return;

            if (m_ForceRebuild)
            {
                m_Points = null;
                m_Ports = null;
                m_EdgeViews = null;
                m_ForceRebuild = false;
                m_Dirty = true;
            }
            else if (m_Points != null && (m_LastZoom != currentZoom || m_LastPanOffset != m_Graph.currentPanOffset))
            {
                m_Points = null;
                m_EdgeViews = null;
                m_Dirty = true;
            }
            m_LastZoom = currentZoom;
            m_LastPanOffset = m_Graph.currentPanOffset;

            if (m_NodeViews != null && m_NodeViews.Count != m_Graph.values.Count)
            {
                nodeViews.Clear();
                foreach (var item in m_Graph.values)
                {
                    CreateNodeView(item);
                }
                m_Points = null;
                m_Ports = null;
                m_EdgeViews = null;
                m_Dirty = true;
            }
            m_Points ??= points;
            m_Ports ??= ports;
            m_EdgeViews ??= edgeViews;
            if (!m_Dirty) return;
            CalculateAllPointRects();
            CalculateAllConnectionCurves();
            UpdateVirtualPointsIsOccupiedStates();
            CheckAllNodesForErrors();
            m_Dirty = false;
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
            ev.bezierPoints = null; // invalidate cached Bezier points so they get recalculated
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
            var zoomedPanOffset = m_Graph.currentPanOffset / currentZoom;

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
    
    
    public abstract partial class GraphWindow<T> : GraphWindow where T : GraphBase
    {
        public T m_Graph => base.m_Graph as T;
        protected sealed override GraphBase CreateGraphBase()
        {
            return CreateGraph();
        }

        protected abstract T CreateGraph();
        
        protected sealed override GraphBase LoadGraphBase()
        {
            return LoadGraph();
        }

        protected abstract T LoadGraph();
    }
}