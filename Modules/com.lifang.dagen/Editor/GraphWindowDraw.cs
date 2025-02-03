using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            foreach (var edgeView in edgeViews.Values)
            {
                if (edgeView.outputNode == null || edgeView.inputNode == null)
                {
                    continue;
                }

                //if both nodes are not visible -> do not draw the edge
                if (!m_NodeViews[edgeView.inputNode.id].isVisible && !m_NodeViews[edgeView.inputNode.id].isVisible)
                {
                    continue;
                }

                if (edgeView.outputVirtualPoint == null || edgeView.inputVirtualPoint == null)
                {
                    continue;
                }

                DrawEdgeCurve(edgeView);
            }
        }

        private void DrawEdgeCurve(EdgeView edge)
        {
            //connect
            m_ConnectionAlpha = 1f;
            if (m_Mode == GraphMode.Connect) m_ConnectionAlpha = 0.5f;

            m_NormalColor = UColor.GetColor().edgeNormalColor;
            m_OutputColor = UColor.GetColor().edgeOutputColor;
            m_InputColor = UColor.GetColor().edgeInputColor;
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
            if (EditorApplication.isPlaying)
            {
                if (m_Graph.GetEdge(edge.edgeId).ping)
                {
                    m_EdgeColor = m_OutputColor;
                    m_AnimateOutput = true;
                    if (m_Graph.GetEdge(edge.edgeId).reSetTime)
                    {
                        m_AniTime = 0;
                        m_Graph.GetEdge(edge.edgeId).reSetTime = false;
                    }
                }
                else if (m_Graph.GetEdge(edge.edgeId).ping)
                {
                    m_EdgeColor = m_InputColor;
                    m_AnimateInput = true;
                    if (m_Graph.GetEdge(edge.edgeId).reSetTime)
                    {
                        m_AniTime = 0;
                        m_Graph.GetEdge(edge.edgeId).reSetTime = false;
                    }
                }
            }
            else if (m_Graph.GetEdge(edge.edgeId).ping ||
                     m_Graph.GetEdge(edge.edgeId).ping)
            {
                m_Graph.GetEdge(edge.edgeId).ping = false;
                m_Graph.GetEdge(edge.edgeId).ping = false;
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

            //HandleUtility.handleMaterial.SetPass(0);
            Handles.DrawBezier(edge.outputVirtualPoint.rect.position,
                edge.inputVirtualPoint.rect.position,
                edge.outputTangent,
                edge.inputTangent,
                m_ConnectionBackgroundColor,
                null,
                currentCurveWidth + 2);
            Handles.DrawBezier(edge.outputVirtualPoint.rect.position,
                edge.inputVirtualPoint.rect.position,
                edge.outputTangent,
                edge.inputTangent,
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
                (int)(Vector2.Distance(edge.outputVirtualPoint.rect.position, edge.inputVirtualPoint.rect.position) *
                      3);
            if (m_NumberOfPoints <= 0) return;
            m_BezierPoints = Handles.MakeBezierPoints(edge.outputVirtualPoint.rect.position,
                edge.inputVirtualPoint.rect.position,
                edge.outputTangent,
                edge.inputTangent,
                m_NumberOfPoints);
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
            if (m_Graph.GetEdge(edge.edgeId).ping && m_DotPointIndex >= m_NumberOfPoints)
            {
                m_Graph.GetEdge(edge.edgeId).ping = false;
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

        #region DrawNodes

        private void DrawNodes(Rect graphArea)
        {
            BeginWindows();
            foreach (var nodeView in m_NodeViews.Values)
            {
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
            foreach (var port in ports.Values)
            {
                DrawPortEdgePoints(port); //draw the edge points
            }
        }

        private void DrawPortEdgePoints(Port port)
        {
            foreach (var virtualPoint in points[port.id])
            {
                var mouseIsOverThisPoint = virtualPoint == m_CurrentHoveredVirtualPoint;
                if (m_AltKeyPressedAnimBool.faded > 0.5f && virtualPoint.isConnected)
                {
                    //set the virtualPoint delete color to RED
                    //set the virtualPoint style to show to the dev that he can disconnect the socket (it's a minus sign)
                    DrawEdgePoint(virtualPoint, mouseIsOverThisPoint, Styles.GetStyle("ConnectionPointMinus"),
                        Color.red);
                    continue;
                }

                var pointColor = port.IsInput() ? UColor.GetColor().portInputColor : UColor.GetColor().portOutputColor;
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
            var pointsInWorldSpace = GetPortEdgePointsInWorldSpace(port, parentNode);
            float minDistance = 100000;
            var worldPosition = parentNode.GetPosition();
            foreach (Vector2 edgePointWorldPosition in pointsInWorldSpace)
            {
                float currentDistance = Vector2.Distance(edgePointWorldPosition, mousePosition);
                if (currentDistance > minDistance) continue;
                worldPosition = edgePointWorldPosition;
                minDistance = currentDistance;
            }

            return worldPosition;
        }

        private IEnumerable<Vector2> GetPortEdgePointsInWorldSpace(Port port, NodeBase parentNode)
        {
            var pointsInWorldSpace = new List<Vector2>();
            if (port == null) return pointsInWorldSpace;
            if (parentNode == null) return pointsInWorldSpace;
            foreach (Vector2 edgePoint in port.GetEdgePoints())
            {
                var socketWorldRect = new Rect(parentNode.GetX(),
                    parentNode.GetY() + port.GetY(),
                    port.GetWidth(),
                    port.GetHeight());

                socketWorldRect.position +=
                    m_Graph.currentPanOffset / currentZoom; //this is the calculated socketGridRect
                pointsInWorldSpace.Add(new Vector2(socketWorldRect.x + edgePoint.x + 8,
                    socketWorldRect.y + edgePoint.y + 8));
            }

            return pointsInWorldSpace;
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