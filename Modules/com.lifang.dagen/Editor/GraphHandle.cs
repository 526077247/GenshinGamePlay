using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DaGenGraph.Editor
{
    public abstract partial class GraphWindow
    {
        #region Private Variables And Properties

        private List<NodeBase> m_SelectedNodes = new List<NodeBase>();
        private bool m_SpaceKeyDown;
        internal bool altKeyPressed;
        private AnimBool m_AltKeyPressedAnimBool;
        private NodeBase m_PreviousHoveredNode;
        private NodeBase m_CurrentHoveredNode;
        private Port m_PreviousHoveredPort;
        private Port m_CurrentHoveredPort;
        private Port m_ActivePort;
        private VirtualPoint m_CurrentHoveredVirtualPoint;
        private Vector2 m_LastMousePosition = Vector2.zero;
        private Vector2 m_DragNodesDistance = Vector2.zero;
        private readonly Dictionary<NodeBase, Vector2> m_InitialDragNodePositions = new Dictionary<NodeBase, Vector2>();
        private Rect m_SelectionRect;
        private SelectPoint m_StartSelectPoint;
        private List<NodeBase> m_TempNodesList;
        private List<NodeBase> m_SelectedNodesWhileSelecting;
        private const float double_click_interval = 0.2f;
        private double m_LastClickTime;

        private bool registeredDoubleClick
        {
            get
            {
                var doubleClick =
                    EditorApplication.timeSinceStartup - m_LastClickTime <
                    double_click_interval; //check if this click happened in the double click interval
                m_LastClickTime = EditorApplication.timeSinceStartup; //update the last click time
                return doubleClick; //return TRUE if a double click has been registered
            }
        }

        #endregion

        #region HandleZoom

        private void HandleZoom()
        {
            //get the current event
            var current = Event.current;
            //check that the developer is scrolling the mouse wheel
            if (current.type != EventType.ScrollWheel) return;
            //get the current mouse position
            var mousePosition = current.mousePosition;

            //zoom out
            if (current.delta.y > 0 && m_CurrentZoom > 0.1f)
            {
                currentZoom -= 0.1f;

                //perform pan to zoom out from mouse position
                m_Graph.currentPanOffset = new Vector2(
                    m_Graph.currentPanOffset.x +
                    (mousePosition.x - position.width / 2 + WorldToGridPosition(mousePosition).x) *
                    0.1f,
                    m_Graph.currentPanOffset.y +
                    (mousePosition.y - position.height / 2 + WorldToGridPosition(mousePosition).y) * 0.1f);
            }

            //zoom in
            if (current.delta.y < 0 && currentZoom < 2f)
            {
                currentZoom += 0.1f;

                //perform pan to zoom in to mouse position
                m_Graph.currentPanOffset = new Vector2(
                    m_Graph.currentPanOffset.x -
                    (mousePosition.x - position.width / 2 + WorldToGridPosition(mousePosition).x) * 0.1f,
                    m_Graph.currentPanOffset.y -
                    (mousePosition.y - position.height / 2 + WorldToGridPosition(mousePosition).y) * 0.1f);
            }

            //clamp the zoom value between min and max zoom targets
            currentZoom = Mathf.Clamp(currentZoom, 0.1f, 2f);
            //set the max number of decimals
            currentZoom = (float)Math.Round(currentZoom, 2);
            //use this event
            current.Use();
        }

        protected Vector2 WorldToGridPosition(Vector2 worldPosition)
        {
            return m_Graph.WorldToGridPosition(worldPosition);
        }

        #endregion

        #region HandlePanning

        private void HandlePanning()
        {
            var current = Event.current;

            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Space)
            {
                m_SpaceKeyDown = true;
                EditorGUIUtility.SetWantsMouseJumping(1);
            }

            if (current.rawType != EventType.KeyUp || current.keyCode != KeyCode.Space) return;
            m_SpaceKeyDown = false;
            EditorGUIUtility.SetWantsMouseJumping(0);
            if (m_Mode == GraphMode.Pan) m_Mode = GraphMode.None;
        }

        #endregion

        #region HandleMouseHover

        protected virtual void HandleMouseHover()
        {
            switch (m_Mode)
            {
                case GraphMode.Select:
                case GraphMode.Drag:
                case GraphMode.Pan:
                    m_CurrentHoveredNode = null;
                    m_CurrentHoveredPort = null;
                    m_CurrentHoveredVirtualPoint = null;
                    return;
                case GraphMode.None: break;
                case GraphMode.Connect: break;
                case GraphMode.Delete: break;
            }


            m_CurrentHoveredNode = null;
            m_CurrentHoveredPort = null;
            m_CurrentHoveredVirtualPoint = null;

            m_CurrentHoveredVirtualPoint = GetVirtualPointAtWorldPosition(Event.current.mousePosition);

            if (m_CurrentHoveredVirtualPoint == null)
            {
                m_CurrentHoveredPort = GetPortAtWorldPositionFromHoverRect(Event.current.mousePosition);

                if (m_CurrentHoveredPort == null)
                {
                    m_CurrentHoveredNode = GetNodeAtWorldPosition(Event.current.mousePosition);
                }
            }

            if (m_CurrentHoveredPort != null)
            {
                //show hover over a socket only if not in delete mode OR if in delete mode AND the socket can be deleted
                if (!altKeyPressed || altKeyPressed &&
                    nodeViews[m_CurrentHoveredPort.nodeId].node.CanDeletePort(m_CurrentHoveredPort))
                {
                    m_CurrentHoveredPort.showHover.target = true;
                    m_CurrentHoveredNode = m_Graph.FindNode(m_CurrentHoveredPort.nodeId);
                }

                Repaint();
            }

            if (m_PreviousHoveredPort != null && m_PreviousHoveredPort != m_CurrentHoveredPort)
            {
                m_PreviousHoveredPort.showHover.target = false;
            }

            if (m_CurrentHoveredNode != null &&
                (GetNodeHeaderGridRect(m_CurrentHoveredNode).Contains(Event.current.mousePosition) ||
                 GetNodeFooterGridRect(m_CurrentHoveredNode).Contains(Event.current.mousePosition)))
            {
                m_CurrentHoveredNode.isHovered = true;
                Repaint();
            }

            if (m_PreviousHoveredNode != null &&
                m_PreviousHoveredNode != m_CurrentHoveredNode)
            {
                m_PreviousHoveredNode.isHovered = false;
            }

            m_PreviousHoveredNode = m_CurrentHoveredNode;
            m_PreviousHoveredPort = m_CurrentHoveredPort;
        }

        private VirtualPoint GetVirtualPointAtWorldPosition(Vector2 worldPosition)
        {
            if (m_CurrentZoom <= 0.4f)
                return null; //if the graph is too zoomed out do not search for connection points

            //in order to help the developer click on these points, we create a small rect at the mouse position and Overlap with it
            //this technique makes for a better use experience when clicking on small items
            float size = 8;
            var mouseRect = new Rect(worldPosition.x,
                worldPosition.y,
                size,
                size);

            mouseRect.x -= size / 2;
            mouseRect.y -= size / 2;


            foreach (var key in points.Keys)
            {
                foreach (var point in points[key])
                {
                    if (!m_NodeViews[point.node.id].isVisible)
                        continue; //the node, that his point belongs to, is not visible -> do not process it
                    var pointGridRect = new Rect(point.rect.position * currentZoom,
                        point.rect.size * currentZoom);
                    pointGridRect.x -= pointGridRect.width / 2;
                    pointGridRect.y -= pointGridRect.height / 4;
                    if (pointGridRect.Overlaps(mouseRect)) return point;
                }
            }


            return null;
        }

        private Port GetPortAtWorldPositionFromHoverRect(Vector2 worldPosition)
        {
            if (currentZoom <= 0.4f)
            {
                //if the graph is too zoomed out do not search for connection points
                return null;
            }

            foreach (var port in ports.Values)
            {
                var node = m_NodeViews[port.nodeId];
                if (node == null) continue;
                if (!node.isVisible)
                {
                    continue; //the node, that the socket belongs to, is not visible -> do not process it
                }

                var worldRect = new Rect(
                    node.node.GetX() +
                    (node.node.GetWidth() - port.hoverRect.width) /
                    2, //(nodeParent.GetWidth() - socket.HoverRect.width) / 2 -> is the X offset of the hover rect
                    node.node.GetY() + port.GetY() +
                    (port.GetHeight() - port.hoverRect.height) /
                    2, //(socket.GetHeight() - socket.HoverRect.height) / 2 -> is the Y offset of the hover rect
                    port.hoverRect.width,
                    port.hoverRect.height);

                var gridRect = new Rect(worldRect.position * currentZoom + m_Graph.currentPanOffset,
                    worldRect.size * currentZoom);

                if (gridRect.Contains(worldPosition))
                    return port;
            }

            return null;
        }

        private NodeBase GetNodeAtWorldPosition(Vector2 worldPosition)
        {
            return (from nodeView in m_NodeViews.Values
                where nodeView.isVisible
                where GetNodeGridRect(nodeView.node).Contains(worldPosition)
                select nodeView.node).FirstOrDefault();
        }

        private Rect GetNodeHeaderGridRect(NodeBase node)
        {
            return new Rect(GridToWorldPosition(node.GetHeaderRect().position),
                node.GetHeaderRect().size * currentZoom);
        }

        private Rect GetNodeFooterGridRect(NodeBase node)
        {
            return new Rect(GridToWorldPosition(node.GetFooterRect().position),
                node.GetFooterRect().size * currentZoom);
        }

        private Vector2 GridToWorldPosition(Vector2 gridPosition)
        {
            return gridPosition * currentZoom + m_Graph.currentPanOffset;
        }

        private Rect GetNodeGridRect(NodeBase node)
        {
            return new Rect(GridToWorldPosition(node.GetRect().position), node.GetRect().size * currentZoom);
        }

        #endregion

        #region HandleMouseRightClicks

        protected virtual void HandleMouseRightClicks()
        {
            var current = Event.current;
            if (!current.isMouse) return;
            if (current.type != EventType.MouseUp) return;
            if (current.button != 1) return;

            if (m_CurrentHoveredPort != null)
            {
                ShowPortContextMenu(m_CurrentHoveredPort);
                return;
            }

            if (m_CurrentHoveredNode != null)
            {
                ShowNodeContextMenu(m_CurrentHoveredNode);
                return;
            }

            ShowGraphContextMenu();
            current.Use();
        }

        protected virtual void ShowGraphContextMenu()
        {
            var menu = new GenericMenu();
            AddGraphMenuItems(menu);
            menu.ShowAsContext();
        }

        protected virtual void AddGraphMenuItems(GenericMenu menu)
        {
            
        }
        
        protected virtual void ShowNodeContextMenu(NodeBase nodeBase)
        {
            var menu = new GenericMenu();
            AddNodeMenuItems(menu,nodeBase);
            menu.ShowAsContext();
        }

        protected virtual void AddNodeMenuItems(GenericMenu menu,NodeBase nodeBase)
        {
            if (nodeBase.canBeDeleted)
            {
                menu.AddItem(new GUIContent("Delete"), false, () => { DeleteNode(nodeBase); });
            }

            menu.AddItem(new GUIContent("SetDefault"), false, () =>
            {
                m_SelectedNodes.Clear();
                m_Graph.startNodeId = nodeBase.id;
            });
        }
        
        protected virtual void ShowPortContextMenu(Port port, bool isLine = false)
        {
            if (isLine && port.edges.Count == 1 && port.IsConnected())
            {
                var res = EditorUtility.DisplayDialog("提示", "确认断开连线？", "是", "否");
                if(res) DisconnectPort(port);
                return;
            }
            var menu = new GenericMenu();
            AddPortMenuItems(menu,port,isLine);
            menu.ShowAsContext();
        }

        protected virtual void AddPortMenuItems(GenericMenu menu, Port port, bool isLine = false)
        {
            if (!isLine && port.IsConnected())
            {
                for (int i = 0; i < port.edges.Count; i++)
                {
                    menu.AddItem(new GUIContent("DisConnectAll"), false, () =>
                    {
                        DisconnectPort(port);
                    });
                }
            }
        }
        #endregion

        #region HandleMouseMiddleClicks

        protected virtual void HandleMouseMiddleClicks()
        {
            Event current = Event.current;
            if (current.button != 2) return;

            switch (current.type)
            {
                case EventType.MouseDown:
                    m_Mode = GraphMode.Pan;
                    break;
                case EventType.MouseUp:
                    m_Mode = GraphMode.None;
                    break;
            }

            DoPanning(current);
        }

        private void DoPanning(Event current)
        {
            switch (current.rawType)
            {
                case EventType.MouseDown:
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                    break;
                case EventType.MouseUp:
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    m_Graph.currentPanOffset += current.delta;
                    current.Use();
                    break;
            }
        }

        #endregion

        #region HandleMouseLeftClicks

        protected virtual void HandleMouseLeftClicks()
        {
            Event current = Event.current;
            if (!current.isMouse) return;
            if (current.button != 0) return;
            if (current.type == EventType.MouseDown)
            {
                //left mouse button is down and the space key is down as well -> enter panning mode
                if (m_SpaceKeyDown)
                {
                    m_Mode = GraphMode.Pan;
                    current.Use();
                    return;
                }

                //pressed left mouse button over a socket point -> but we have at least two nodes selected -> do not allow starting any connections
                if (m_SelectedNodes.Count < 2)
                {
                    if (m_CurrentHoveredVirtualPoint != null)
                    {
                        if (altKeyPressed)
                        {
                            //pressed left mouse button over a socket virtual point while holding down Alt -> Disconnect Virtual Point
                            DisconnectVirtualPoint(m_CurrentHoveredVirtualPoint);
                        }
                        else
                        {
                            //pressed left mouse button over a socket connection point -> it's a possible start of a connection
                            m_ActivePort = m_CurrentHoveredVirtualPoint.port; //set the socket as the active socket
                            m_Mode = GraphMode.Connect; //set the graph in connection mode
                        }

                        current.Use();
                        return;
                    }
                }

                //pressed left mouse button over a node -> check to see if it's inside the header (if no node is currently selected) or it just over a node (if at least 2 nodes are selected)
                if (m_CurrentHoveredNode != null)
                {
                    if (GetNodeGridRect(m_CurrentHoveredNode)
                            .Contains(Event.current.mousePosition) || //if mouse is inside node -> allow dragging
                        m_SelectedNodes.Count >
                        1) //OR if there are at least 2 nodes selected -> allow dragging from any point on the node
                    {
                        if (!m_DrawInspector || Event.current.mousePosition.x < position.width - m_NodeInspectorWidth)
                        {
                            //pressed left mouse button over a node -> select/deselect it
                            if (current.shift || current.control || current.command)
                            {
                                //add/remove the node to/from selection
                                SelectNodes(new List<NodeBase> {  }, true);
                            }
                            //we may have a selection and we do not want to override it in order to be able to start dragging
                            else if (!m_SelectedNodes.Contains(m_CurrentHoveredNode))
                            {
                                //select this node only
                                SelectNodes(new List<NodeBase> { m_CurrentHoveredNode }, false);
                            }

                            //allow dragging ONLY IF the mouse is over a selected node
                            //in the previous lines we only checked if it's over a node, but not if the node we are hovering over is currently selected
                            if (m_SelectedNodes.Contains(m_CurrentHoveredNode))
                            {
                                //pressed left mouse button over a node -> it's a possible start drag
                                PrepareToDragSelectedNodes(Event.current.mousePosition);
                                m_Mode = GraphMode.Drag;
                            }
                        }
                    }

                    current.Use();
                    return;
                }

                //pressed left mouse button over nothing -> it's a possible start selection
                PrepareToCreateSelectionBox(Event.current.mousePosition);
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag)
            {
                //left mouse click is dragging and the graph is in panning mode
                if (m_Mode == GraphMode.Pan)
                {
                    //check that the space key is held down -> otherwise exit pan mode
                    if (!m_SpaceKeyDown)
                    {
                        m_Mode = GraphMode.None;
                    }
                    else
                    {
                        DoPanning(current);
                    }

                    current.Use();
                    return;
                }

                //mouse left click is dragging a connection
                if (m_Mode == GraphMode.Connect)
                {
                    //mouse is over a socket -> color the line to green if connection is possible or red otherwise
                    if (m_CurrentHoveredPort != null)
                    {
                        m_CreateEdgeLineColor = m_ActivePort.CanConnect(m_CurrentHoveredPort,m_Graph) ? Color.green : Color.red;
                    }
                    //mouse is over a socket connection point -> color the line to green if connection is possible or red otherwise   
                    else if (m_CurrentHoveredVirtualPoint != null)
                    {
                        m_CreateEdgeLineColor = m_ActivePort.CanConnect(m_CurrentHoveredVirtualPoint.port,m_Graph)
                            ? Color.green
                            : Color.red;
                    }
                    //mouse is not over anything connectable -> show the connection point color to look for 
                    else
                    {
                        m_CreateEdgeLineColor = m_ActivePort.IsInput()
                            ? UColor.GetColor().edgeInputColor
                            : UColor.GetColor().edgeOutputColor;
                    }

                    current.Use();
                    return;
                }

                //mouse left click is dragging one or more nodes
                if (m_Mode == GraphMode.Drag)
                {
                    UpdateSelectedNodesWhileDragging();
                    current.Use();
                    return;
                }

                //mouse left click is dragging and creating a selection box <- we know this because the the mouse is not over a point nor a node
                if (m_StartSelectPoint != null)
                {
                    if (!m_DrawInspector || Event.current.mousePosition.x < position.width - m_NodeInspectorWidth)
                    {
                        m_Mode = GraphMode.Select;
                    }
                }
                
                if (m_Mode == GraphMode.Select)
                {
                    UpdateSelectionBox(Event.current.mousePosition);
                    UpdateSelectBoxSelectedNodesWhileSelecting(current);
                    UpdateNodesSelectedState(m_SelectedNodesWhileSelecting);
                    current.Use();
                    return;
                }
            }

            if (current.type == EventType.MouseUp)
            {
                if (registeredDoubleClick)
                {
                    if (m_PreviousHoveredNode != null)
                    {
                        m_NodeViews[m_PreviousHoveredNode.id].OnDoubleClick(this);
                    }
                }

                foreach (var item in m_NodeViews)
                {
                    if (item.Key != m_PreviousHoveredNode?.id)
                    {
                        item.Value.OnUnFocus(this);
                    }
                }

                //lifted left mouse button and was panning (space key was/is down) -> reset graph to idle
                if (m_Mode == GraphMode.Pan)
                {
                    m_Mode = GraphMode.None;
                    current.Use();
                    return;
                }

                //lifted left mouse button and was dragging -> reset graph to idle
                if (m_Mode == GraphMode.Drag)
                {
                    m_InitialDragNodePositions.Clear();
                    m_Mode = GraphMode.None;
                    current.Use();
                    return;
                }

                //lifted left mouse button and was selecting via selection box -> end selections and reset graph to idle mode
                if (m_Mode == GraphMode.Select)
                {
                    EndDragSelectedNodes();
                    m_Mode = GraphMode.None;
                    current.Use();
                    return;
                }

                //check if this happened over another socket or connection point
                if (m_CurrentHoveredPort != null)
                {
                    //lifted left mouse button over a socket
                    if (m_ActivePort != null && //if there is an active socket
                        m_ActivePort != m_CurrentHoveredPort && //and it's not this one
                        m_ActivePort.CanConnect(m_CurrentHoveredPort,m_Graph)) //and the two sockets can get connected
                    {
                        ConnectPorts(m_ActivePort, m_CurrentHoveredPort); //connect the two sockets
                    }

                    m_ActivePort = null; //clear the active socket
                    m_Mode = GraphMode.None; //set the graph in idle mode
                    current.Use();
                    return;
                }

                if (m_CurrentHoveredVirtualPoint != null)
                {
                    //lifted left mouse button over a socket connection point
                    if (m_ActivePort != null && //if there is an active socket
                        m_ActivePort != m_CurrentHoveredVirtualPoint.port && //and it's not this one
                        m_ActivePort.CanConnect(m_CurrentHoveredVirtualPoint.port,m_Graph)) //and the two sockets can get connected
                    {
                        ConnectPorts(m_ActivePort, m_CurrentHoveredVirtualPoint.port); //connect the two sockets
                    }

                    m_ActivePort = null; //clear the active socket
                    m_Mode = GraphMode.None; //set the graph in idle mode
                    current.Use();
                    return;
                }

                if (m_ActivePort != null && m_CurrentHoveredPort == null && m_CurrentHoveredVirtualPoint == null)
                {
                    var port = m_ActivePort;
                    ShowPortContextMenu(port, true);
                    m_ActivePort = null; //clear the active socket
                    m_Mode = GraphMode.None; //set the graph in idle mode
                    current.Use();
                    return;
                }

                //it a connecting process was under way -> clear it
                //lifted left mouse button, but no virtual point was under the mouse position
                if (m_Mode == GraphMode.Connect)
                {
                    m_ActivePort = null; //clear the active socket
                    m_Mode = GraphMode.None; //set the graph in idle mode
                }

                var node = GetNodeAtWorldPosition(Event.current.mousePosition);
                if (node != null)
                {
                    m_Mode = GraphMode.None; //set the graph in idle mode
                    current.Use();
                    return;
                }

               
                if(!m_DrawInspector || Event.current.mousePosition.x<position.width-m_NodeInspectorWidth)
                    //lifted mouse left button over nothing -> deselect all and select the graph itself
                    ExecuteGraphAction(GraphAction.DeselectAll); //deselect all nodes and select the graph itself
                m_Mode = GraphMode.None; //set the graph in idle mode
                current.Use();
                return;
            }

            //check if the developer released the left mouse button outside of the graph window
            if (current.rawType == EventType.MouseUp || current.rawType == EventType.MouseLeaveWindow)
                switch (m_Mode)
                {
                    case GraphMode.Select:
                        EndDragSelectedNodes();
                        m_Mode = GraphMode.None;
                        current.Use();
                        break;
                }
        }

        private void DisconnectVirtualPoint(VirtualPoint virtualPoint)
        {
            if (!virtualPoint.port.IsConnected()) return;
            if (!virtualPoint.isConnected) return;

            var edgeViewList = new List<EdgeView>();
            foreach (var edgeView in edgeViews.Values)
            {
                if (edgeView.inputVirtualPoint == virtualPoint || edgeView.outputVirtualPoint == virtualPoint)
                {
                    edgeViewList.Add(edgeView);
                }
            }

            foreach (var ev in edgeViewList)
            {
                ev.outputPort.RemoveEdge(ev.edgeId);
                ev.inputPort.RemoveEdge(ev.edgeId);
                edgeViews.Remove(ev.edgeId);
            }
        }

        private void SelectNodes(IEnumerable<NodeBase> nodes, bool addToCurrentSelection)
        {
            if (!addToCurrentSelection) m_SelectedNodes.Clear(); //if this is a new selection -> clear the previous one

            //check if the GUI needs to get reconstructed
            foreach (NodeBase node in nodes)
            {
                //found a node id not in the constructed database -> skip this entry and trigger a construct gui
                if (!nodeViews.ContainsKey(node.id))
                {
                    continue;
                }

                if (m_SelectedNodes.Contains(node))
                {
                    m_SelectedNodes.Remove(node);
                    continue;
                }

                m_SelectedNodes.Add(node);
            }

            //update the currently selected nodes visual (normal style or selected style)
            UpdateNodesSelectedState(m_SelectedNodes);
        }

        private void PrepareToDragSelectedNodes(Vector2 mousePosition)
        {
            m_LastMousePosition = mousePosition;
            m_DragNodesDistance = Vector2.zero;
            m_InitialDragNodePositions.Clear();
            foreach (NodeBase node in m_SelectedNodes) m_InitialDragNodePositions.Add(node, node.GetPosition());
        }

        private void PrepareToCreateSelectionBox(Vector2 currentMousePosition)
        {
            currentMousePosition = currentMousePosition / currentZoom;

            m_SelectionRect = new Rect(); //clear the previous selection rect
            m_StartSelectPoint = new SelectPoint(currentMousePosition); //get the start position for the selection rect
            m_TempNodesList = new List<NodeBase>(); //clear the previous saved start selection box selected nodes
            m_TempNodesList =
                new List<NodeBase>(
                    m_SelectedNodes); //save the currently selected node (we need this in order to be able to invert the selection)
            m_SelectedNodesWhileSelecting = new List<NodeBase>(); //clear the nodes contained in the previous selection
        }

        private void UpdateSelectedNodesWhileDragging()
        {
            m_DragNodesDistance += Event.current.mousePosition - m_LastMousePosition;
            m_LastMousePosition = Event.current.mousePosition;

            foreach (NodeBase node in m_SelectedNodes)
            {
                Vector2 initialPosition = m_InitialDragNodePositions[node];
                Vector2 newPosition = node.GetPosition();
                newPosition.x = initialPosition.x + m_DragNodesDistance.x / currentZoom;
                newPosition.y = initialPosition.y + m_DragNodesDistance.y / currentZoom;
                node.SetPosition(SnapPositionToGrid(newPosition));
            }

            UpdateVirtualPointsIsOccupiedStates();
        }

        private void UpdateSelectionBox(Vector2 currentMousePosition)
        {
            currentMousePosition = currentMousePosition / currentZoom;

            if (m_StartSelectPoint == null)
            {
                m_SelectionRect = new Rect();
                return;
            }

            //calculate the selection rect position and size
            float lx = Mathf.Max(m_StartSelectPoint.x, currentMousePosition.x);
            float ly = Mathf.Max(m_StartSelectPoint.y, currentMousePosition.y);
            float sx = Mathf.Min(m_StartSelectPoint.x, currentMousePosition.x);
            float sy = Mathf.Min(m_StartSelectPoint.y, currentMousePosition.y);
            m_SelectionRect = new Rect(sx, sy, lx - sx, ly - sy); //set the new values to the selection rect
        }

        private void UpdateSelectBoxSelectedNodesWhileSelecting(Event e)
        {
            //update the current selection
            m_SelectedNodesWhileSelecting.Clear();
            foreach (var nodeView in nodeViews.Values)
            {
                var gridRect = new Rect(nodeView.node.GetPosition() + m_Graph.currentPanOffset / currentZoom,
                    nodeView.node.GetSize());
                if (m_SelectionRect.Overlaps(gridRect))
                    m_SelectedNodesWhileSelecting.Add(nodeView.node);
            }

            //shift is pressed -> toggle selection
            if (e.shift)
            {
                foreach (var tempNode in m_TempNodesList)
                {
                    if (!m_SelectedNodesWhileSelecting.Contains(tempNode))
                    {
                        m_SelectedNodesWhileSelecting.Add(tempNode);
                    }
                    else
                    {
                        m_SelectedNodesWhileSelecting.Remove(tempNode);
                    }
                }
            }
        }

        private void UpdateNodesSelectedState(ICollection<NodeBase> selectedNodes)
        {
            foreach (var nodeView in m_NodeViews.Values)
            {
                nodeView.isSelected = selectedNodes.Contains(nodeView.node);
            }
        }

        private void EndDragSelectedNodes()
        {
            var selectionBoxNodes = new List<NodeBase>();

            foreach (NodeBase selectedNode in m_SelectedNodesWhileSelecting) selectionBoxNodes.Add(selectedNode);

            //if shift key is not pressed, clear current selection
            SelectNodes(selectionBoxNodes, false);

            m_StartSelectPoint = null;
        }

        protected void ConnectPorts(Port outputPort, Port inputPort)
        {
            if (outputPort.OverrideConnection()) DisconnectPort(outputPort);
            if (inputPort.OverrideConnection()) DisconnectPort(inputPort);
            ConnectPorts(m_Graph, outputPort, inputPort);
        }

        private static void ConnectPorts(GraphBase graph, Port outputPort, Port inputPort)
        {
            if (outputPort.OverrideConnection())
            {
                outputPort.Disconnect();
            }

            if (inputPort.OverrideConnection())
            {
                inputPort.Disconnect();
            }

            graph.CreateEdge(outputPort, inputPort);
        }

        public void DisconnectPort(Port port)
        {
            if (!port.IsConnected()) return;

            var portEdgeIds = port.GetEdgeIds();
            foreach (var edgeId in portEdgeIds)
            {
                if (!edgeViews.ContainsKey(edgeId)) continue;
                var ev = edgeViews[edgeId];
                if (ev == null)
                {
                    edgeViews.Remove(edgeId);
                    m_Graph.RemoveEdge(edgeId);
                    continue;
                }

                if (ev.outputPort != null)
                {
                    ev.outputPort.RemoveEdge(edgeId);
                }

                if (ev.inputPort != null)
                {
                    ev.inputPort.RemoveEdge(edgeId);
                }

                edgeViews.Remove(edgeId);
                m_Graph.RemoveEdge(edgeId);
            }
        }

        private static Vector2 SnapPositionToGrid(Vector2 position)
        {
            int xCell = Mathf.RoundToInt(position.x / 12f);
            int yCell = Mathf.RoundToInt(position.y / 12f);
            position.x = xCell * 12f;
            position.y = yCell * 12f;
            return position;
        }

        private void UpdateVirtualPointsIsOccupiedStates()
        {
            //mark all virtual points a not connected
            foreach (var pointList in points.Values)
            {
                foreach (var point in pointList)
                {
                    point.isConnected = false;
                }
            }

            //go through all the virtual connections and mark their virtual points as connected 
            foreach (EdgeView ev in edgeViews.Values)
            {
                if (ev == null) continue;
                if (ev.inputVirtualPoint == null) continue;
                if (ev.outputVirtualPoint == null) continue;

                ev.inputVirtualPoint.isConnected = true;
                ev.outputVirtualPoint.isConnected = true;

                //UPDATE THE ACTUAL CONNECTION POSITIONS IN THE PORT -> we do this here as we are already doing an enumeration and is a bit more efficient no to do this twice
                //this is a very important step, as we update the positions of the input and output connection points in the connections found on the port
                foreach (var edgeId in ev.inputPort.edges)
                    if (edgeId == ev.edgeId)
                    {
                        var edge = m_Graph.GetEdge(edgeId);
                        edge.inputEdgePoint = ev.inputVirtualPoint.localPointPosition;
                        edge.outputEdgePoint = ev.outputVirtualPoint.localPointPosition;
                    }

                foreach (var edgeId in ev.outputPort.edges)
                    if (edgeId == ev.edgeId)
                    {
                        var edge = m_Graph.GetEdge(edgeId);
                        edge.inputEdgePoint = ev.inputVirtualPoint.localPointPosition;
                        edge.outputEdgePoint = ev.outputVirtualPoint.localPointPosition;
                    }
            }
        }

        #endregion

        #region ExecuteGraphAction

        private void ExecuteGraphAction(GraphAction graphAction)
        {
            switch (graphAction)
            {
                case GraphAction.DeselectAll:
                    DeselectAll();
                    break;
                case GraphAction.Copy:
                    break;
                case GraphAction.Connect:
                    break;
                case GraphAction.DeleteNodes:
                    DeleteNodes(m_SelectedNodes);
                    break;
                case GraphAction.Disconnect:
                    break;
                case GraphAction.Paste:
                    break;
                case GraphAction.SelectAll:
                    SelectAll();
                    break;
                case GraphAction.SelectNodes:
                    break;
                default:
                    Debug.LogWarning("ExecuteErrorGraphAction: "+graphAction);
                    break;
            }
        }

        private void DeselectAll()
        {
            m_SelectedNodes.Clear();
            UpdateNodesSelectedState(m_SelectedNodes);
        }

        private void SelectAll()
        {
            m_SelectedNodes.Clear();
            m_SelectedNodes.AddRange(m_Graph.values);
            UpdateNodesSelectedState(m_SelectedNodes);
        }

        protected virtual void DeleteNode(NodeBase node)
        {
            if (node == null || !node.canBeDeleted) return;
            var startNode = m_Graph.GetStartNode();
            //disconnect all the nodes that need to be deleted

            foreach (EdgeView edgeView in edgeViews.Values)
            {
                if (edgeView == null) continue;

                if (edgeView.inputNode == node && edgeView.outputPort != null)
                {
                    edgeView.outputPort.DisconnectFromNode(node.id, m_Graph);
                }

                if (edgeView.outputNode == node && edgeView.inputPort != null)
                {
                    edgeView.inputPort.DisconnectFromNode(node.id, m_Graph);
                }
            }

            for (int i = node.inputPorts.Count-1; i >=0 ; i--)
            {
                RemovePort(node.inputPorts[i], true);
            }
            for (int i = node.outputPorts.Count-1; i >=0 ; i--)
            {
                RemovePort(node.outputPorts[i], true);
            }
            //at this point the nodes have been disconnected
            //'delete' the nodes by adding them the the DeletedNodes list
            m_Graph.RemoveNode(node.id);
            nodeViews.Remove(node.id);

            DeselectAll();
            if (startNode == node)
            {
                if (m_Graph.values.Count > 0)
                {
                    m_Graph.startNodeId = m_Graph.values.First().id;
                }
                else
                {
                    m_Graph.startNodeId = null;
                }
            }
        }

        protected virtual void DeleteNodes(List<NodeBase> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            var startNode = m_Graph.GetStartNode();
            nodes = nodes.Where(n => n != null).ToList();
            //disconnect all the nodes that need to be deleted
            foreach (var node in nodes)
            {
                if (!node.canBeDeleted) continue;

                foreach (EdgeView edgeView in edgeViews.Values)
                {
                    if (node == null || edgeView == null) continue;

                    if (edgeView.inputNode == node && edgeView.outputPort != null)
                    {
                        edgeView.outputPort.DisconnectFromNode(node.id, m_Graph);
                    }

                    if (edgeView.outputNode == node && edgeView.inputPort != null)
                    {
                        edgeView.inputPort.DisconnectFromNode(node.id, m_Graph);
                    }
                }
            }

            //at this point the nodes have been disconnected
            //'delete' the nodes by adding them the the DeletedNodes list
            foreach (var node in nodes)
            {
                if (node == null) continue;
                if (!node.canBeDeleted) continue;
                m_Graph.RemoveNode(node.id);
                nodeViews.Remove(node.id);
            }

            DeselectAll();
            if (nodes.Contains(startNode))
            {
                if (m_Graph.values.Count > 0)
                {
                    m_Graph.startNodeId = m_Graph.values.First().id;
                }
                else
                {
                    m_Graph.startNodeId = null;
                }
            }
        }

        protected virtual NodeView CreateNodeView(NodeBase node)
        {
            if (node == null) return null;
            var typeAttributes = node.GetType().GetCustomAttributes(true);
            Type viewType = FallBackNodeViewType();
            foreach (var attr in typeAttributes)
            {
                if (attr is NodeViewTypeAttribute nodeViewTypeAttribute && viewType.IsAssignableFrom(nodeViewTypeAttribute.ViewType))
                {
                    viewType = nodeViewTypeAttribute.ViewType;
                    break;
                }
            }
            
            var nodeView = Activator.CreateInstance(viewType) as NodeView;
            if (nodeView == null) return null;
            nodeView.Init(++m_Graph.windowID, node, m_Graph, this);
            m_NodeViews.Add(node.id, nodeView);
            return nodeView;
        }

        protected virtual Type FallBackNodeViewType()
        {
            return typeof(NodeView);
        }

        protected virtual void RemovePort(Port port,bool force = false)
        {
            if (!force && !m_Graph.FindNode(port.nodeId).CanDeletePort(port)) return;
            DisconnectPort(port);
            points.Remove(port.id);
            edgeViews.Remove(port.id);
           
            var node = m_Graph.FindNode(port.nodeId);
            if (node == null) return;
            node.DeletePort(port);
        }

        protected abstract void SaveGraph();
        
        protected void LoadGraph()
        {
            var graphBase = LoadGraphBase();
            SetGraph(graphBase);
        }
        protected abstract GraphBase LoadGraphBase();
        
        protected virtual void InitGraph()
        {
            var graphBase = CreateGraphBase();
            SetGraph(graphBase);
        }

        protected abstract GraphBase CreateGraphBase();

        protected virtual void SetGraph(GraphBase graphBase)
        {
            if (graphBase == null) return;
            if (string.IsNullOrEmpty(graphBase.name))
            {
                graphBase.name = graphBase.GetType().Name;
            }
            nodeViews.Clear();
            m_Graph = graphBase;
            foreach (var item in m_Graph.values)
            {
                CreateNodeView(item);
            }
        }
        #endregion

        #region HandleKeys

        private void HandleKeys()
        {
            if (!m_HasFocus) return;
            var e = Event.current;

            //Alt Key down -> hide selections
            if (altKeyPressed && e.alt)
                UpdateNodesSelectedState(new List<NodeBase>());
            else if (altKeyPressed != e.alt)
                UpdateNodesSelectedState(m_SelectedNodes);
            altKeyPressed = e.alt && m_HasFocus;
            m_AltKeyPressedAnimBool.target = altKeyPressed;
            if (e.type != EventType.KeyUp) return;
            switch (e.keyCode)
            {
                case KeyCode.N: //Create new UINode
                    ExecuteGraphAction(GraphAction.CreateNode);
                    break;
                case KeyCode.Escape: //Cancel (Escape)
                    switch (m_Mode)
                    {
                        case GraphMode.None:
                            ExecuteGraphAction(GraphAction.DeselectAll);
                            break;
                        case GraphMode.Connect:
                            m_ActivePort = null;
                            break;
                        case GraphMode.Select:
                            m_Mode = GraphMode.None;
                            break;
                        case GraphMode.Drag:
                            ExecuteGraphAction(GraphAction.DeselectAll);
                            break;
                    }

                    m_Mode = GraphMode.None;
                    break;

                case KeyCode.C: //Copy
                    if (e.control) ExecuteGraphAction(GraphAction.Copy);
                    break;

                case KeyCode.V: //Paste
                    if (e.control) ExecuteGraphAction(GraphAction.Paste);
                    break;

                case KeyCode.Delete: //Delete
                    if (Event.current.mousePosition.x < position.width - m_NodeInspectorWidth)
                    {
                        ExecuteGraphAction(GraphAction.DeleteNodes);
                    }
                    break;
                case KeyCode.A: //Select All
                    if (e.control || e.command) ExecuteGraphAction(GraphAction.SelectAll);
                    break;
            }
        }

        #endregion

        #region OnGameObjectsDragIn

        /// <summary>
        /// GameObject Drag In Graph 
        /// </summary>
        /// <param name="dragGameObjects"></param>
        /// <param name="pos"></param>
        protected virtual void OnGameObjectsDragIn(GameObject[] dragGameObjects, Vector2 pos)
        {
        }

        #endregion
    }
}