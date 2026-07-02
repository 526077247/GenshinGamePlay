using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DaGenGraph.Editor
{
    public abstract partial class GraphWindow
    {
        #region GUIStyles

        private static GUIStyle s_DotStyle;
        private static GUIStyle s_ConnectionPointOverrideEmpty;
        private static GUIStyle s_ConnectionPointOverrideConnected;
        private static GUIStyle s_ConnectionPointMultipleEmpty;
        private static GUIStyle s_ConnectionPointMultipleConnected;
        private static GUIStyle s_ConnectionPointMinus;

        private static GUIStyle dotStyle
        {
            get { return s_DotStyle ?? (s_DotStyle = Styles.GetStyle("NodeDot")); }
        }

        private static GUIStyle edgePointOverrideEmpty
        {
            get
            {
                return s_ConnectionPointOverrideEmpty ??
                       (s_ConnectionPointOverrideEmpty = Styles.GetStyle("ConnectionPointOverrideEmpty"));
            }
        }

        private static GUIStyle edgePointOverrideConnected
        {
            get
            {
                return s_ConnectionPointOverrideConnected ?? (s_ConnectionPointOverrideConnected =
                    Styles.GetStyle("ConnectionPointOverrideConnected"));
            }
        }

        private static GUIStyle edgePointMultipleEmpty
        {
            get
            {
                return s_ConnectionPointMultipleEmpty ??
                       (s_ConnectionPointMultipleEmpty = Styles.GetStyle("ConnectionPointMultipleEmpty"));
            }
        }

        private static GUIStyle edgePointMultipleConnected
        {
            get
            {
                return s_ConnectionPointMultipleConnected ?? (s_ConnectionPointMultipleConnected =
                    Styles.GetStyle("ConnectionPointMultipleConnected"));
            }
        }

        private static GUIStyle edgePointMinus
        {
            get { return s_ConnectionPointMinus ?? (s_ConnectionPointMinus = Styles.GetStyle("ConnectionPointMinus")); }
        }

        #endregion

        #region Private Variables

        private delegate void DrawToolbarHandler();

        private List<DrawToolbarHandler> m_DrawToolbarHandlers=new List<DrawToolbarHandler>();
        private List<DrawToolbarHandler> m_DrawToolbarHandlersRight=new List<DrawToolbarHandler>();
        private Color m_CreateEdgeLineColor = Color.white;
        private Color m_ConnectionBackgroundColor;
        private Color m_EdgeColor;
        private Color m_DotColor;
        private Color m_InputColor;
        private Color m_NormalColor;
        private Color m_OutputColor;
        private Vector3 m_DotPoint;
        private Vector3[] m_BezierPoints;
        private float m_ConnectionAlpha;
        private float m_DotSize;
        private int m_DotPointIndex;
        private int m_NumberOfPoints;
       

        #endregion

        #region DrawEdges

        private void DrawEdges()
        {
            if (currentZoom <= 0.2f) return;
            if (m_NodeViews == null) return;
            m_AniTime += m_Timer;
            if (m_AniTime >= 3)
            {
                m_AniTime = 0;
            }

            var visibleGridRect = new Rect(
                -m_Graph.currentPanOffset.x / currentZoom,
                -m_Graph.currentPanOffset.y / currentZoom,
                m_ScaledGraphArea.width,
                m_ScaledGraphArea.height);

            foreach (var edgeView in edgeViews.Values)
            {
                if (edgeView.outputNode == null || edgeView.inputNode == null)
                {
                    continue;
                }

                //if both nodes are not visible -> do not draw the edge
                if (!nodeViews.TryGetValue(edgeView.inputNode.id, out var inNv) || !nodeViews.TryGetValue(edgeView.outputNode.id, out var outNv))
                    continue;
                if (!inNv.isVisible && !outNv.isVisible)
                {
                    continue;
                }

                // Skip edges where both endpoints are inside the same collapsed group
                if (IsEdgeWithinCollapsedGroup(edgeView))
                    continue;

                if (edgeView.outputVirtualPoint == null || edgeView.inputVirtualPoint == null)
                {
                    continue;
                }

                // viewport culling: only skip edges when both endpoints are to the same side of the visible area
                var outNodeRect = edgeView.outputNode.GetRect();
                var inNodeRect = edgeView.inputNode.GetRect();
                if (outNodeRect.xMax < visibleGridRect.xMin && inNodeRect.xMax < visibleGridRect.xMin) continue;
                if (outNodeRect.xMin > visibleGridRect.xMax && inNodeRect.xMin > visibleGridRect.xMax) continue;
                if (outNodeRect.yMax < visibleGridRect.yMin && inNodeRect.yMax < visibleGridRect.yMin) continue;
                if (outNodeRect.yMin > visibleGridRect.yMax && inNodeRect.yMin > visibleGridRect.yMax) continue;

                DrawEdgeCurve(edgeView);
            }
        }

        private void DrawEdgeCurve(EdgeView edge)
        {
            //connect
            m_ConnectionAlpha = 1f;
            if (m_Mode == GraphMode.Connect) m_ConnectionAlpha = 0.5f;

            var uc = UColor.GetColor();
            m_NormalColor = uc.edgeNormalColor;
            m_OutputColor = uc.edgeOutputColor;
            m_InputColor = uc.edgeInputColor;
            m_NormalColor.a = m_ConnectionAlpha;
            m_OutputColor.a = m_ConnectionAlpha;
            m_InputColor.a = m_ConnectionAlpha;
            m_EdgeColor = m_NormalColor;
            m_AnimateInput = false;
            m_AnimateOutput = false;

            //A node is selected and the Alt Key is not pressed -> show the edge color depending on socket type of this node (if it is an output or an input one)
            if (m_SelectedNodes.Count == 1 && !altKeyPressed)
            {
                NodeBase selectedNode = m_SelectedNodes[0];
                if (selectedNode == null) return;
                if (selectedNode.ContainsEdge(edge.edgeId))
                {
                    if (selectedNode == edge.outputNode)
                    {
                        //color for output edge
                        m_EdgeColor = m_OutputColor;
                        m_AnimateOutput = true;
                    }

                    if (selectedNode == edge.inputNode)
                    {
                        //color for input edge
                        m_EdgeColor = m_InputColor;
                        m_AnimateInput = true;
                    }
                }
            }

            float currentCurveWidth = 3;
            var edgeData = m_Graph.GetEdge(edge.edgeId);
            if (edgeData == null) return;
            if (EditorApplication.isPlaying)
            {
                if (edgeData.ping)
                {
                    m_EdgeColor = m_OutputColor;
                    m_AnimateOutput = true;
                    if (edgeData.reSetTime)
                    {
                        m_AniTime = 0;
                        edgeData.reSetTime = false;
                    }
                }
                else if (edgeData.ping)
                {
                    m_EdgeColor = m_InputColor;
                    m_AnimateInput = true;
                    if (edgeData.reSetTime)
                    {
                        m_AniTime = 0;
                        edgeData.reSetTime = false;
                    }
                }
            }
            else if (edgeData.ping)
            {
                edgeData.ping = false;
            }

            m_DotColor = m_EdgeColor;
            if (altKeyPressed) //delete mode is enabled -> check if we should color the edge to RED
            {
                //check this edge's points by testing if the mouse if hovering over one of this edge's virtual points
                bool colorTheConnectionRed = edge.inputVirtualPoint == m_CurrentHoveredVirtualPoint ||
                                             edge.outputVirtualPoint == m_CurrentHoveredVirtualPoint;
                if (colorTheConnectionRed)
                {
                    //set the edge color to RED -> as the developer might want to remove this edge
                    m_EdgeColor = Color.red;
                    //make the red curve just a tiny bit more thick to make it stand out even better
                    currentCurveWidth += 1;
                }
            }

            m_ConnectionBackgroundColor = new Color(m_EdgeColor.r * 0.2f,
                m_EdgeColor.g * 0.2f,
                m_EdgeColor.b * 0.2f,
                m_ConnectionAlpha - 0.2f);

            // Determine actual start/end positions, redirecting to collapsed group port if needed
            var outputCollapsedGroup = GetCollapsedGroupOfNode(edge.outputNode.id);
            var inputCollapsedGroup = GetCollapsedGroupOfNode(edge.inputNode.id);
            Vector2 outputPos, inputPos;
            Vector2 outputTan, inputTan;

            if (outputCollapsedGroup != null && outputCollapsedGroup.collapsedPortScreenPos.TryGetValue(edge.outputPort.id, out var outPortPos))
            {
                outputPos = outPortPos;
                outputTan = outputPos + Vector2.right * 50f;
            }
            else
            {
                outputPos = edge.outputVirtualPoint.rect.position;
                outputTan = edge.outputTangent;
            }
            if (inputCollapsedGroup != null && inputCollapsedGroup.collapsedPortScreenPos.TryGetValue(edge.inputPort.id, out var inPortPos))
            {
                inputPos = inPortPos;
                inputTan = inputPos + Vector2.left * 50f;
            }
            else
            {
                inputPos = edge.inputVirtualPoint.rect.position;
                inputTan = edge.inputTangent;
            }

            //HandleUtility.handleMaterial.SetPass(0);
            Handles.DrawBezier(outputPos,
                inputPos,
                outputTan,
                inputTan,
                m_ConnectionBackgroundColor,
                null,
                currentCurveWidth + 2);
            Handles.DrawBezier(outputPos,
                inputPos,
                outputTan,
                inputTan,
                m_EdgeColor,
                null,
                currentCurveWidth);
            //if the window does not have focus -> return (DO NOT PLAY ANIMATION)
            //if (!m_HasFocus) return;
            //if the mouse is not inside the window -> return (DO NOT PLAY ANIMATION)
            //if (!MouseInsideWindow) return; 
            if (!m_AnimateInput && !m_AnimateOutput)
            {
                //if the animation is not enabled for both points -> return (DO NOT PLAY ANIMATION)
                return;
            }

            //points multiplier - useful for a smooth dot travel - smaller means fewer travel point (makes the point 'jumpy') and higher means more travel points (make the point move smoothly)
            m_NumberOfPoints =
                (int)(Vector2.Distance(outputPos, inputPos) * 3);
            if (m_NumberOfPoints <= 0) return;
            if (edge.bezierPoints == null || edge.bezierPoints.Length != m_NumberOfPoints)
            {
                edge.bezierPoints = Handles.MakeBezierPoints(outputPos,
                    inputPos,
                    outputTan,
                    inputTan,
                    m_NumberOfPoints);
            }
            m_BezierPoints = edge.bezierPoints;
            m_DotPointIndex = 0;
            //we set the number of points as the bezierPoints length - 1
            m_NumberOfPoints--;
            if (m_AnimateInput)
            {
                m_DotPointIndex = (int)(m_AniTime * m_NumberOfPoints);
            }
            else if (m_AnimateOutput)
            {
                m_DotPointIndex = m_NumberOfPoints - (int)((1 - m_AniTime) * m_NumberOfPoints);
            }

            m_DotPointIndex = Mathf.Clamp(m_DotPointIndex, 0, m_NumberOfPoints);
            //reset edge's ping
            if (edgeData.ping && m_DotPointIndex >= m_NumberOfPoints)
            {
                edgeData.ping = false;
            }

            m_DotPoint = m_BezierPoints[m_DotPointIndex];
            m_DotSize = currentCurveWidth * 2;
            //make the dot a bit brighter
            m_DotColor = new Color(m_DotColor.r * 1.2f, m_DotColor.g * 1.2f, m_DotColor.b * 1.2f, m_DotColor.a);

            GUI.color = m_DotColor;
            GUI.Box(new Rect(m_DotPoint.x - m_DotSize / 2, m_DotPoint.y - m_DotSize / 2, m_DotSize, m_DotSize), "",
                dotStyle);
            GUI.color = Color.white;
        }

        #endregion

        #region DrawGroups

        private static readonly Color s_GroupBgExpanded = new Color(0.3f, 0.6f, 1.0f, 0.12f);
        private static readonly Color s_GroupBorderExpanded = new Color(0.3f, 0.6f, 1.0f, 0.6f);
        private static readonly Color s_GroupTitleBarExpanded = new Color(0.3f, 0.6f, 1.0f, 0.35f);
        private static readonly Color s_GroupBgCollapsed = new Color(0.2f, 0.45f, 0.85f, 0.9f);
        private static readonly Color s_GroupBorderCollapsed = new Color(0.15f, 0.35f, 0.7f, 1f);
        private static readonly Color s_GroupTitleBarCollapsed = new Color(0.15f, 0.35f, 0.7f, 1f);

        private void DrawGroups()
        {
            if (m_Graph?.groups == null || m_Graph.groups.Count == 0) return;

            foreach (var group in m_Graph.groups)
            {
                if (group == null) continue;
                DrawGroup(group);
            }
        }

        private void DrawGroup(NodeGroup group)
        {
            var nodes = GetGroupNodes(group);
            var rect = group.GetCurrentRect(nodes);
            // Match node coordinate convention: grid + panOffset/zoom (GUI.Window does this internally)
            rect.position += m_Graph.currentPanOffset / currentZoom;

            if (group.isCollapsed)
            {
                DrawCollapsedGroup(group, rect);
            }
            else
            {
                DrawExpandedGroup(group, rect);
            }
        }

        private void DrawExpandedGroup(NodeGroup group, Rect rect)
        {
            // Background
            var oldColor = GUI.color;
            GUI.color = s_GroupBgExpanded;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

            // Border
            DrawGroupBorder(rect, s_GroupBorderExpanded);

            // Title bar
            var titleRect = new Rect(rect.x, rect.y, rect.width, NodeGroup.k_TitleBarHeight);
            GUI.color = s_GroupTitleBarExpanded;
            GUI.DrawTexture(titleRect, EditorGUIUtility.whiteTexture);
            GUI.color = oldColor;

            // Title text (click to edit)
            var titleLabelRect = new Rect(titleRect.x + 8, titleRect.y + 4, titleRect.width - 50, titleRect.height - 8);
            if (group.isTitleEditing)
            {
                GUI.SetNextControlName("GroupTitleEdit_" + group.id);
                group.titleEditBuffer = EditorGUI.TextField(titleLabelRect, group.titleEditBuffer ?? group.title);
                var e = Event.current;
                if (e is { type: EventType.KeyUp, keyCode: KeyCode.Return })
                {
                    group.title = group.titleEditBuffer;
                    group.isTitleEditing = false;
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUI.Label(titleLabelRect, group.title, new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft });
            }

            // Collapse button "−"
            var btnRect = new Rect(titleRect.x + titleRect.width - 28, titleRect.y + 3, 22, 22);
            if (GUI.Button(btnRect, "−", EditorStyles.miniButton))
            {
                CollapseGroup(group, rect);
            }

            GUI.color = oldColor;
        }

        private void DrawCollapsedGroup(NodeGroup group, Rect rect)
        {
            var oldColor = GUI.color;

            // Collect external ports and compute positions
            var externalPorts = GetExternalPorts(group);
            var inputPorts = new List<Port>();
            var outputPorts = new List<Port>();
            foreach (var p in externalPorts)
            {
                if (p.IsInput()) inputPorts.Add(p);
                else outputPorts.Add(p);
            }
            int maxPortsPerSide = Mathf.Max(inputPorts.Count, outputPorts.Count);
            float portRowHeight = 20f;
            float neededHeight = NodeGroup.k_TitleBarHeight + 16f + maxPortsPerSide * portRowHeight + 8f;
            if (neededHeight > rect.height)
                rect.height = neededHeight;

            group.collapsedPortScreenPos.Clear();
            group.collapsedPorts.Clear();

            // Background (solid)
            GUI.color = s_GroupBgCollapsed;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

            // Border
            DrawGroupBorder(rect, s_GroupBorderCollapsed);

            // Title bar
            var titleRect = new Rect(rect.x, rect.y, rect.width, NodeGroup.k_TitleBarHeight);
            GUI.color = s_GroupTitleBarCollapsed;
            GUI.DrawTexture(titleRect, EditorGUIUtility.whiteTexture);
            GUI.color = oldColor;

            // Title text
            var titleTextRect = new Rect(titleRect.x + 8, titleRect.y + 2, titleRect.width - 50, titleRect.height - 4);
            if (group.isTitleEditing)
            {
                GUI.SetNextControlName("GroupTitleEdit_" + group.id);
                group.titleEditBuffer = EditorGUI.TextField(titleTextRect, group.titleEditBuffer ?? group.title);
                var e = Event.current;
                if (e is { type: EventType.KeyUp, keyCode: KeyCode.Return })
                {
                    group.title = group.titleEditBuffer;
                    group.isTitleEditing = false;
                    GUI.FocusControl(null);
                }
            }
            else
            {
                var collapsedTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = Color.white }
                };
                GUI.Label(titleTextRect, group.title, collapsedTitleStyle);
            }

            // Expand button "+"
            var btnRect = new Rect(titleRect.x + titleRect.width - 28, titleRect.y + 3, 22, 22);
            if (GUI.Button(btnRect, "+", EditorStyles.miniButton))
            {
                ExpandGroup(group);
            }

            // Subtitle showing member count
            var subRect = new Rect(rect.x + 8, rect.y + NodeGroup.k_TitleBarHeight + 4, rect.width - 16, 20);
            var subStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(1, 1, 1, 0.7f) } };
            GUI.Label(subRect, $"{group.nodeIds.Count} node(s) collapsed", subStyle);

            // Draw input ports on the left side
            float dotSize = 12f;
            float inputStartY = rect.y + NodeGroup.k_TitleBarHeight + 28f;
            for (int i = 0; i < inputPorts.Count; i++)
            {
                var port = inputPorts[i];
                float portY = inputStartY + i * portRowHeight;
                DrawCollapsedPort(group, port, rect, portY, dotSize, isInput: true);
            }

            // Draw output ports on the right side
            float outputStartY = rect.y + NodeGroup.k_TitleBarHeight + 28f;
            for (int i = 0; i < outputPorts.Count; i++)
            {
                var port = outputPorts[i];
                float portY = outputStartY + i * portRowHeight;
                DrawCollapsedPort(group, port, rect, portY, dotSize, isInput: false);
            }

            GUI.color = oldColor;
        }

        private void DrawCollapsedPort(NodeGroup group, Port port, Rect rect, float portY, float dotSize, bool isInput)
        {
            if (port == null || group == null) return;
            var oldColor = GUI.color;
            float xPos = isInput ? rect.x : rect.x + rect.width;
            Vector2 portScreenPos = new Vector2(xPos, portY);
            group.collapsedPortScreenPos ??= new Dictionary<string, Vector2>();
            group.collapsedPorts ??= new Dictionary<string, Port>();
            group.collapsedPortScreenPos[port.id] = portScreenPos;
            group.collapsedPorts[port.id] = port;

            // Port dot
            var dotRect = new Rect(portScreenPos.x - dotSize / 2f, portScreenPos.y - dotSize / 2f, dotSize, dotSize);
            var portColor = isInput ? UColor.GetColor().portInputColor : UColor.GetColor().portOutputColor;
            portColor.a = 1f;
            GUI.color = portColor;
            GUI.Box(dotRect, GUIContent.none, dotStyle);
            GUI.color = oldColor;

            // Port label — default includes node id for disambiguation
            string label = port.portName ?? "Port";
            var ownerNode = m_Graph.FindNode(port.nodeId);
            if (ownerNode != null && ownerNode.id != null && ownerNode.id.Length >= 6)
                label = $"{label} [{ownerNode.id.Substring(0, 6)}]";
            if (group.collapsedPortLabels != null && group.collapsedPortLabels.TryGetValue(port.id, out var customLabel))
                label = customLabel;
            var labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1, 1, 1, 0.9f) } };
            var labelRect = isInput
                ? new Rect(portScreenPos.x + dotSize / 2f + 4, portScreenPos.y - 8, rect.width / 2 - 20, 16)
                : new Rect(portScreenPos.x - rect.width / 2 + dotSize, portScreenPos.y - 8, rect.width / 2 - 20, 16);
            labelStyle.alignment = isInput ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;

            // Store the label rect for right-click hit testing
            group.collapsedPortScreenPos["label_" + port.id] = labelRect.position;
            group.collapsedPortScreenPos["labelSize_" + port.id] = new Vector2(labelRect.width, labelRect.height);

            // Inline rename
            if (s_PortRenameId == port.id)
                {
                    s_PortRenameBuffer = EditorGUI.TextField(labelRect, s_PortRenameBuffer);
                    var ev = Event.current;
                    if (ev is { type: EventType.KeyUp, keyCode: KeyCode.Return })
                    {
                        if (!string.IsNullOrEmpty(s_PortRenameBuffer))
                        {
                            PushUndoSnapshot();
                            group.collapsedPortLabels[port.id] = s_PortRenameBuffer;
                        }
                        s_PortRenameId = null;
                        m_Dirty = true;
                        GUI.FocusControl(null);
                    }
                    if (ev is { type: EventType.KeyUp, keyCode: KeyCode.Escape })
                    {
                        s_PortRenameId = null;
                        GUI.FocusControl(null);
                    }
                }
                else
                {
                    GUI.Label(labelRect, label, labelStyle);
                }

            GUI.color = oldColor;
        }

        private void DrawGroupBorder(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            float borderWidth = 2f;
            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, borderWidth), EditorGUIUtility.whiteTexture);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - borderWidth, rect.width, borderWidth), EditorGUIUtility.whiteTexture);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, borderWidth, rect.height), EditorGUIUtility.whiteTexture);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - borderWidth, rect.y, borderWidth, rect.height), EditorGUIUtility.whiteTexture);
            GUI.color = oldColor;
        }

        #endregion

        #region Group Helpers

        private List<NodeBase> GetGroupNodes(NodeGroup group)
        {
            var result = new List<NodeBase>();
            if (group?.nodeIds == null) return result;
            foreach (var nodeId in group.nodeIds)
            {
                var node = m_Graph.FindNode(nodeId);
                if (node != null) result.Add(node);
            }
            return result;
        }

        private bool IsNodeInCollapsedGroup(string nodeId)
        {
            if (m_Graph?.groups == null) return false;
            for (int i = 0; i < m_Graph.groups.Count; i++)
            {
                var g = m_Graph.groups[i];
                if (g != null && g.isCollapsed && g.nodeIds.Contains(nodeId))
                    return true;
            }
            return false;
        }

        private bool IsNodeInAnyGroup(string nodeId)
        {
            if (m_Graph?.groups == null) return false;
            for (int i = 0; i < m_Graph.groups.Count; i++)
            {
                var g = m_Graph.groups[i];
                if (g != null && !g.isCollapsed && g.nodeIds.Contains(nodeId))
                    return true;
            }
            return false;
        }

        private bool IsEdgeWithinCollapsedGroup(EdgeView ev)
        {
            if (ev.inputNode == null || ev.outputNode == null) return false;
            return IsNodeInCollapsedGroup(ev.inputNode.id) && IsNodeInCollapsedGroup(ev.outputNode.id);
        }

        private NodeGroup GetCollapsedGroupOfNode(string nodeId)
        {
            if (m_Graph?.groups == null) return null;
            for (int i = 0; i < m_Graph.groups.Count; i++)
            {
                var g = m_Graph.groups[i];
                if (g != null && g.isCollapsed && g.nodeIds.Contains(nodeId))
                    return g;
            }
            return null;
        }

        /// <summary> Collect ports of member nodes that have edges to non-member nodes </summary>
        private List<Port> GetExternalPorts(NodeGroup group)
        {
            var result = new List<Port>();
            if (group?.nodeIds == null) return result;
            var nodeIdSet = new HashSet<string>(group.nodeIds);
            foreach (var nodeId in group.nodeIds)
            {
                var node = m_Graph.FindNode(nodeId);
                if (node == null) continue;
                foreach (var port in node.inputPorts)
                {
                    if (port == null || !port.IsConnected()) continue;
                    for (int i = 0; i < port.edges.Count; i++)
                    {
                        var edge = m_Graph.GetEdge(port.edges[i]);
                        if (edge != null && !nodeIdSet.Contains(edge.outputNodeId))
                        {
                            result.Add(port);
                            break;
                        }
                    }
                }
                foreach (var port in node.outputPorts)
                {
                    if (port == null || !port.IsConnected()) continue;
                    for (int i = 0; i < port.edges.Count; i++)
                    {
                        var edge = m_Graph.GetEdge(port.edges[i]);
                        if (edge != null && !nodeIdSet.Contains(edge.inputNodeId))
                        {
                            result.Add(port);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private void CollapseGroup(NodeGroup group, Rect expandedRect)
        {
            PushUndoSnapshot();
            // expandedRect is in node-GUI space (grid + panOffset/zoom) — convert to grid for storage
            var panOffset = m_Graph.currentPanOffset / currentZoom;
            group.x = expandedRect.center.x - group.collapsedWidth / 2f - panOffset.x;
            group.y = expandedRect.center.y - group.collapsedHeight / 2f - panOffset.y;
            group.isCollapsed = true;
            m_EdgeViews = null;
            m_Dirty = true;
            Repaint();
        }

        private void ExpandGroup(NodeGroup group)
        {
            PushUndoSnapshot();
            group.isCollapsed = false;
            m_EdgeViews = null;
            m_Dirty = true;
            Repaint();
        }

        #endregion

        #region DrawNodes

        private void DrawNodes(Rect graphArea)
        {
            BeginWindows();
            var visibleGridRect = new Rect(
                -m_Graph.currentPanOffset.x / currentZoom,
                -m_Graph.currentPanOffset.y / currentZoom,
                m_ScaledGraphArea.width,
                m_ScaledGraphArea.height);
            foreach (var nodeView in m_NodeViews.Values)
            {
                if (nodeView?.node == null) continue;
                if (IsNodeInCollapsedGroup(nodeView.node.id)) continue;
                if (!nodeView.node.GetRect().Overlaps(visibleGridRect)) continue;
                nodeView.isInGroup = IsNodeInAnyGroup(nodeView.node.id);
                nodeView.zoomedBeyondPortDrawThreshold = currentZoom <= 0.4f;
                nodeView.DrawNodeGUI(graphArea, m_Graph.currentPanOffset, currentZoom);
            }

            EndWindows();
        }

        #endregion

        #region DrawPortsEdgePoints

        private void DrawPortsEdgePoints()
        {
            if (currentZoom <= 0.4f) return;
            var visibleGridRect = new Rect(
                -m_Graph.currentPanOffset.x / currentZoom,
                -m_Graph.currentPanOffset.y / currentZoom,
                m_ScaledGraphArea.width,
                m_ScaledGraphArea.height);
            foreach (var port in ports.Values)
            {
                if (IsNodeInCollapsedGroup(port.nodeId)) continue;
                if (!nodeViews.TryGetValue(port.nodeId, out var nv) || nv?.node == null) continue;
                var node = nv.node;
                var portGridRect = new Rect(node.GetX(), node.GetY() + port.GetY(), port.GetWidth(), port.GetHeight());
                if (!portGridRect.Overlaps(visibleGridRect)) continue;
                DrawPortEdgePoints(port);
            }
        }

        private void DrawPortEdgePoints(Port port)
        {
            var uc = UColor.GetColor();
            var inputColor = uc.portInputColor;
            var outputColor = uc.portOutputColor;
            var minusStyle = edgePointMinus;
            foreach (var virtualPoint in points[port.id])
            {
                var mouseIsOverThisPoint = virtualPoint == m_CurrentHoveredVirtualPoint;
                if (m_AltKeyPressedAnimBool.faded > 0.5f && virtualPoint.isConnected)
                {
                    DrawEdgePoint(virtualPoint, mouseIsOverThisPoint, minusStyle, Color.red);
                    continue;
                }

                var pointColor = port.IsInput() ? inputColor : outputColor;
                GUIStyle pointStyle;
                switch (port.GetConnectionMode())
                {
                    case EdgeMode.Override:
                        pointStyle = virtualPoint.isConnected ? edgePointOverrideConnected : edgePointOverrideEmpty;
                        break;
                    case EdgeMode.Multiple:
                        pointStyle = virtualPoint.isConnected ? edgePointMultipleConnected : edgePointMultipleEmpty;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                DrawEdgePoint(virtualPoint, mouseIsOverThisPoint, pointStyle, pointColor);
            }
        }

        private void DrawEdgePoint(VirtualPoint virtualPoint, bool mouseIsOverThisPoint, GUIStyle pointStyle,
            Color pointColor)
        {
            pointColor.a = virtualPoint.isConnected || mouseIsOverThisPoint ? 1f : 0.8f;
            //this makes an unoccupied connector a bit smaller than an occupied one (looks nicer)
            var occupiedRatioChange = virtualPoint.isConnected ? 1f : 0.8f;
            var pointWidth = 16f * occupiedRatioChange * (mouseIsOverThisPoint ? 1.2f : 1f);
            var pointHeight = 16f * occupiedRatioChange * (mouseIsOverThisPoint ? 1.2f : 1f);
            var pointRect = new Rect(virtualPoint.rect.position.x - pointWidth / 2,
                virtualPoint.rect.position.y - pointHeight / 2,
                pointWidth,
                pointHeight);
            GUI.color = pointColor;
            GUI.Box(pointRect, GUIContent.none, pointStyle);
            GUI.color = Color.white;
        }

        #endregion

        #region DrawLineFromPortToPosition

        private void DrawLineFromPortToPosition(Port activePort, Vector2 worldPosition)
        {
            if (m_Mode != GraphMode.Connect) return;
            if (currentZoom <= 0.4f) return;
            var from = GetClosestEdgePointWorldPositionFromPortToMousePosition(activePort,
                worldPosition / currentZoom);
            var to = worldPosition;
            float edgeLineWidth = 3;
            var edgeBackgroundColor = new Color(m_CreateEdgeLineColor.r * 0.2f,
                m_CreateEdgeLineColor.g * 0.2f, m_CreateEdgeLineColor.b * 0.2f, 0.8f);
            Handles.DrawBezier(from, to, to, from, edgeBackgroundColor, null, edgeLineWidth + 2);
            Handles.DrawBezier(from, to, to, from, m_CreateEdgeLineColor, null, edgeLineWidth);
            var dotSize = edgeLineWidth * 3;
            GUI.color = new Color(m_CreateEdgeLineColor.r * 1.2f, m_CreateEdgeLineColor.g * 1.2f,
                m_CreateEdgeLineColor.b * 1.2f, 1f);
            GUI.Box(new Rect(to.x - dotSize / 2, to.y - dotSize / 2, dotSize, dotSize), "", dotStyle);
            GUI.color = Color.white;
            HandleUtility.Repaint();
            Repaint();
        }

        private Vector2 GetClosestEdgePointWorldPositionFromPortToMousePosition(Port port, Vector2 mousePosition)
        {
            var parentNode = nodeViews[port.nodeId].node;
            if (parentNode == null) return Vector2.zero;
            float minDistance = 100000;
            var worldPosition = parentNode.GetPosition();
            var edgePoints = port.GetEdgePoints();
            var socketWorldRect = new Rect(parentNode.GetX(),
                parentNode.GetY() + port.GetY(),
                port.GetWidth(),
                port.GetHeight());
            socketWorldRect.position += m_Graph.currentPanOffset / currentZoom;
            for (int i = 0; i < edgePoints.Count; i++)
            {
                var edgePoint = edgePoints[i];
                var pt = new Vector2(socketWorldRect.x + edgePoint.x + 8, socketWorldRect.y + edgePoint.y + 8);
                float currentDistance = Vector2.Distance(pt, mousePosition);
                if (currentDistance > minDistance) continue;
                worldPosition = pt;
                minDistance = currentDistance;
            }
            return worldPosition;
        }

        #endregion

        #region DrawSelectionBox

        private void DrawSelectionBox()
        {
            if (m_Mode != GraphMode.Select) return;
            Color initialColor = GUI.color;
            GUI.color = (EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.3f) : new Color(1, 1, 1, 0.3f));
            var sty = Styles.GetStyle("BackgroundSquare");
            GUI.Label(m_SelectionRect, string.Empty, sty);
            GUI.color = initialColor;
        }

        #endregion

        #region DrawToolbar

        protected virtual void AddButton(GUIContent content, Action callback,bool left = true,
            params GUILayoutOption[] options)
        {
            if (left)
            {
                m_DrawToolbarHandlers.Add(() =>
                {
                    if (GUILayout.Button(content, options))
                    {
                        callback.Invoke();
                    }
                });
            }
            else
            {
                m_DrawToolbarHandlersRight.Add(() =>
                {
                    if (GUILayout.Button(content, options))
                    {
                        callback.Invoke();
                    }
                });
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            foreach (var drawToolbarHandler in m_DrawToolbarHandlers)
            {
                drawToolbarHandler.Invoke();
            }

            GUILayout.FlexibleSpace();
            foreach (var drawToolbarHandler in m_DrawToolbarHandlersRight)
            {
                drawToolbarHandler.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region DrawInspector

        private Vector2 nodeScrollPos;
        private Vector2 graphScrollPos;
        private bool foldGraph;
        private Dictionary<string,bool> foldNode = new Dictionary<string, bool>();
        private void DrawInspector(float start,float width)
        {
            bool showNodeView = m_Graph!=null && m_SelectedNodes!=null && m_SelectedNodes.Count>0;
            var inspectorArea = new Rect(start, 20, width, position.height-20);
            GUILayout.BeginArea(inspectorArea);
            foldGraph = EditorGUILayout.BeginFoldoutHeaderGroup(foldGraph, m_Graph.name);
            if (foldGraph)
            {
                graphScrollPos = EditorGUILayout.BeginScrollView(graphScrollPos, GUILayout.Width(inspectorArea.width), GUILayout.Height(showNodeView?200:inspectorArea.height-20));
                DrawGraphInspector();
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (showNodeView)
            {
                nodeScrollPos = EditorGUILayout.BeginScrollView(nodeScrollPos, GUILayout.Width(inspectorArea.width), GUILayout.Height(inspectorArea.height-(foldGraph?225:25)));
                for (int i = 0; i < m_SelectedNodes.Count; i++)
                {
                    var node = m_SelectedNodes[i];
                    if (node == null) continue;
                    if(!nodeViews.TryGetValue(node.id,out var view)) continue;
                    if (!foldNode.TryGetValue(node.id, out var fold))
                    {
                        fold = false;
                        foldNode[m_SelectedNodes[i].id] = fold;
                    }
                    fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, $"{node.name}({node.id})");
                    if (fold)
                    {
                        EditorGUILayout.Space(10);
                        //nodeView自己决定展示
                        view.DrawInspector(true);
                    }
                    foldNode[m_SelectedNodes[i].id] = fold;
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        protected virtual void DrawGraphInspector()
        {
            DrawObjectInspector(m_Graph, true);
        }
        #endregion
    }
}