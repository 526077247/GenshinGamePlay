using System;
using System.Collections.Generic;
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
        private NodeGroup m_DraggingGroup;
        private NodeGroup m_SelectedGroup;
        private Vector2 m_DragGroupStartPos;
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
            m_Dirty = true;
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
            var evtType = Event.current.type;
            if (evtType != EventType.MouseMove && evtType != EventType.MouseDrag && evtType != EventType.MouseDown && evtType != EventType.MouseUp)
            {
                // Keep existing hover state during Layout/Repaint
                if (m_CurrentHoveredPort != null || m_CurrentHoveredNode != null)
                    Repaint();
                return;
            }

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
                    if (!nodeViews.TryGetValue(point.node.id, out var nv) || nv == null || !nv.isVisible)
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
                if (!nodeViews.TryGetValue(port.nodeId, out var node) || node == null) continue;
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
            foreach (var nodeView in m_NodeViews.Values)
            {
                if (!nodeView.isVisible) continue;
                if (GetNodeGridRect(nodeView.node).Contains(worldPosition))
                    return nodeView.node;
            }
            return null;
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
                if (m_Graph == null || m_CurrentHoveredPort.nodeId == null || !nodeViews.ContainsKey(m_CurrentHoveredPort.nodeId))
                {
                    m_CurrentHoveredPort = null;
                }
                else
                {
                    ShowPortContextMenu(m_CurrentHoveredPort);
                    return;
                }
            }

            // Check collapsed group port right-click
            var collapsedPort = GetCollapsedGroupPortAtPosition(current.mousePosition);
            if (collapsedPort != null)
            {
                var portGroup = GetCollapsedGroupOfNode(collapsedPort.nodeId);
                if (portGroup != null)
                {
                    ShowCollapsedPortContextMenu(collapsedPort, portGroup);
                    current.Use();
                    return;
                }
            }

            // Check group right-click (title bar or body)
            var clickedGroup = GetGroupAtWorldPosition(current.mousePosition);
            if (clickedGroup != null)
            {
                ShowGroupContextMenu(clickedGroup);
                current.Use();
                return;
            }

            if (m_CurrentHoveredNode != null)
            {
                if (m_Graph == null || !nodeViews.ContainsKey(m_CurrentHoveredNode.id))
                {
                    m_CurrentHoveredNode = null;
                }
                else
                {
                    ShowNodeContextMenu(m_CurrentHoveredNode);
                    return;
                }
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
            if (s_Clipboard != null && s_Clipboard.nodes.Count > 0)
            {
                menu.AddItem(new GUIContent("Paste"), false, () => { ExecuteGraphAction(GraphAction.Paste); });
            }
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

            if (m_SelectedNodes.Count >= 1)
            {
                menu.AddItem(new GUIContent("Copy"), false, () => { ExecuteGraphAction(GraphAction.Copy); });
            }

            if (s_Clipboard != null && s_Clipboard.nodes.Count > 0)
            {
                menu.AddItem(new GUIContent("Paste"), false, () => { ExecuteGraphAction(GraphAction.Paste); });
            }

            if (m_SelectedNodes.Count >= 2)
            {
                menu.AddItem(new GUIContent("Group Selected Nodes"), false, () => { CreateGroupFromSelection(); });
            }

            // If node is inside a group, show "Remove from Group" options
            if (m_Graph?.groups != null)
            {
                foreach (var group in m_Graph.groups)
                {
                    if (group == null || group.isCollapsed) continue;
                    if (group.nodeIds.Contains(nodeBase.id))
                    {
                        var groupName = group.title;
                        menu.AddItem(new GUIContent($"Remove from Group/{groupName}"), false,
                            () => { RemoveNodeFromGroup(nodeBase.id, group); });
                    }
                }

                // If node is NOT inside a group but is within a group's expanded rect, show "Add to Group"
                var gridPos = WorldToGridPosition(Event.current.mousePosition);
                foreach (var group in m_Graph.groups)
                {
                    if (group == null || group.isCollapsed) continue;
                    if (group.nodeIds.Contains(nodeBase.id)) continue;
                    var nodes = GetGroupNodes(group);
                    var rect = group.GetExpandedRect(nodes);
                    if (rect.Contains(gridPos))
                    {
                        var groupName = group.title;
                        menu.AddItem(new GUIContent($"Add to Group/{groupName}"), false,
                            () => { AddNodeToGroup(nodeBase.id, group); });
                    }
                }
            }
        }
        
        protected virtual void ShowPortContextMenu(Port port, bool isLine = false)
        {
            if (isLine && port.edges.Count == 1 && port.IsConnected())
            {
                var res = EditorUtility.DisplayDialog("提示", "确认断开连线？", "是", "否");
                if(res) { PushUndoSnapshot(); DisconnectPort(port); }
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
                        PushUndoSnapshot();
                        DisconnectPort(port);
                    });
                }
            }

            // If port's node is in an expanded group, offer to rename the collapsed port label
            if (m_Graph?.groups != null)
            {
                foreach (var group in m_Graph.groups)
                {
                    if (group == null || group.isCollapsed) continue;
                    if (group.nodeIds.Contains(port.nodeId))
                    {
                        menu.AddItem(new GUIContent($"Rename in Collapsed View/{group.title}"), false, () =>
                        {
                            s_PortRenameId = port.id;
                            s_PortRenameBuffer = group.collapsedPortLabels.TryGetValue(port.id, out var lbl) ? lbl : port.portName;
                            m_Dirty = true;
                            Repaint();
                        });
                    }
                }
            }
        }
        #endregion

        #region CopyPaste

        private class ClipboardPort
        {
            public string portName;
            public int direction; // 0=Input, 1=Output
            public int edgeMode;
            public int edgeType;
            public bool canBeDeleted;
            public bool canBeReordered;
        }

        private class ClipboardNode
        {
            public System.Type nodeType;
            public float x, y, width, height;
            public string name;
            public List<ClipboardPort> inputPorts = new();
            public List<ClipboardPort> outputPorts = new();
            public Dictionary<string, object> fieldData = new();
        }

        private class ClipboardEdge
        {
            public int outputNodeIndex, outputPortIndex;
            public int inputNodeIndex, inputPortIndex;
        }

        private class ClipboardGroupInfo
        {
            public string title;
            public Dictionary<string, string> collapsedPortLabels = new(); // oldPortId -> label
            public List<int> memberIndices = new();
            public float x, y;
            public bool isCollapsed;
            public Dictionary<string, string> oldPortIdMap = new(); // oldPortId -> "nodeIdx:portIdx"
        }

        private class Clipboard
        {
            public List<ClipboardNode> nodes = new();
            public List<ClipboardEdge> edges = new();
            public List<ClipboardGroupInfo> groups = new();
            public int pasteCount = 0;
        }

        private static Clipboard s_Clipboard;

        private void DoCopy()
        {
            if (m_SelectedNodes == null || m_SelectedNodes.Count == 0) return;
            var clipboard = new Clipboard();
            var selectedIds = new HashSet<string>();
            foreach (var node in m_SelectedNodes)
            {
                if (node != null) selectedIds.Add(node.id);
            }

            // Snapshot nodes
            var nodeIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < m_SelectedNodes.Count; i++)
            {
                var src = m_SelectedNodes[i];
                if (src == null) continue;
                nodeIndexMap[src.id] = i;
                var cn = new ClipboardNode
                {
                    nodeType = src.GetType(),
                    x = src.GetX(),
                    y = src.GetY(),
                    width = src.GetWidth(),
                    height = src.GetHeight(),
                    name = src.name
                };
                foreach (var port in src.inputPorts)
                {
                    if (port == null) continue;
                    cn.inputPorts.Add(new ClipboardPort
                    {
                        portName = port.portName, direction = 0,
                        edgeMode = (int)port.edgeMode, edgeType = (int)port.edgeType,
                        canBeDeleted = port.canBeDeleted, canBeReordered = port.canBeReordered
                    });
                }
                foreach (var port in src.outputPorts)
                {
                    if (port == null) continue;
                    cn.outputPorts.Add(new ClipboardPort
                    {
                        portName = port.portName, direction = 1,
                        edgeMode = (int)port.edgeMode, edgeType = (int)port.edgeType,
                        canBeDeleted = port.canBeDeleted, canBeReordered = port.canBeReordered
                    });
                }
                // Copy data fields via reflection (skip id, ports, system fields)
                var fields = src.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var f in fields)
                {
                    if (f.Name == "id" || f.Name == "inputPorts" || f.Name == "outputPorts" ||
                        f.Name == "x" || f.Name == "y" || f.Name == "width" || f.Name == "height" ||
                        f.Name == "isHovered" || f.Name == "ping" || f.Name == "name" ||
                        f.Name == "canBeDeleted" || f.Name == "errorNodeNameIsEmpty" || f.Name == "errorDuplicateNameFoundInGraph" ||
                        f.Name == "minimumInputPortsCount" || f.Name == "minimumOutputPortsCount" ||
                        f.Name == "allowDuplicateNodeName" || f.Name == "allowEmptyNodeName")
                        continue;
                    cn.fieldData[f.Name] = f.GetValue(src);
                }
                clipboard.nodes.Add(cn);
            }

            // Snapshot internal edges (both endpoints in selection)
            for (int i = 0; i < m_Graph.edges.Count; i++)
            {
                var edge = m_Graph.edges[i];
                if (edge == null) continue;
                if (!nodeIndexMap.ContainsKey(edge.outputNodeId) || !nodeIndexMap.ContainsKey(edge.inputNodeId))
                    continue;
                var outNode = m_Graph.FindNode(edge.outputNodeId);
                var inNode = m_Graph.FindNode(edge.inputNodeId);
                if (outNode == null || inNode == null) continue;
                int outPortIdx = outNode.outputPorts.FindIndex(p => p != null && p.id == edge.outputPortId);
                int inPortIdx = inNode.inputPorts.FindIndex(p => p != null && p.id == edge.inputPortId);
                if (outPortIdx < 0 || inPortIdx < 0) continue;
                clipboard.edges.Add(new ClipboardEdge
                {
                    outputNodeIndex = nodeIndexMap[edge.outputNodeId],
                    outputPortIndex = outPortIdx,
                    inputNodeIndex = nodeIndexMap[edge.inputNodeId],
                    inputPortIndex = inPortIdx
                });
            }

            // Snapshot groups whose members are all selected
            if (m_Graph.groups != null)
            {
                foreach (var group in m_Graph.groups)
                {
                    if (group == null) continue;
                    bool allSelected = true;
                    foreach (var nid in group.nodeIds)
                    {
                        if (!selectedIds.Contains(nid)) { allSelected = false; break; }
                    }
                    if (!allSelected) continue;
                    var cg = new ClipboardGroupInfo
                    {
                        title = group.title,
                        x = group.x, y = group.y,
                        isCollapsed = group.isCollapsed
                    };
                    foreach (var nid in group.nodeIds)
                    {
                        if (nodeIndexMap.TryGetValue(nid, out var idx))
                            cg.memberIndices.Add(idx);
                    }
                    // Build oldPortId -> "nodeIdx:portIdx" for remapping collapsedPortLabels
                    foreach (var nid in group.nodeIds)
                    {
                        if (!nodeIndexMap.TryGetValue(nid, out var nIdx)) continue;
                        var node = m_Graph.FindNode(nid);
                        if (node == null) continue;
                        for (int p = 0; p < node.inputPorts.Count; p++)
                            if (node.inputPorts[p] != null)
                                cg.oldPortIdMap[node.inputPorts[p].id] = $"{nIdx}:i{p}";
                        for (int p = 0; p < node.outputPorts.Count; p++)
                            if (node.outputPorts[p] != null)
                                cg.oldPortIdMap[node.outputPorts[p].id] = $"{nIdx}:o{p}";
                    }
                    // Copy collapsedPortLabels
                    foreach (var kvp in group.collapsedPortLabels)
                        cg.collapsedPortLabels[kvp.Key] = kvp.Value;
                    clipboard.groups.Add(cg);
                }
            }

            clipboard.pasteCount = 0;
            s_Clipboard = clipboard;
            ShowNotification(new GUIContent($"Copied {clipboard.nodes.Count} node(s), {clipboard.edges.Count} edge(s), {clipboard.groups.Count} group(s)"), 1f);
        }

        private void DoPaste()
        {
            if (s_Clipboard == null || s_Clipboard.nodes.Count == 0) return;
            PushUndoSnapshot();
            s_Clipboard.pasteCount++;
            float offset = 50f * s_Clipboard.pasteCount;
            var newNodes = new List<NodeBase>();
            // oldPortKey "nodeIdx:i/o portIdx" -> new Port
            var portMap = new Dictionary<string, Port>();

            // Create nodes
            for (int i = 0; i < s_Clipboard.nodes.Count; i++)
            {
                var cn = s_Clipboard.nodes[i];
                // Create via reflection: m_Graph.CreateNode<T>(pos, name, false)
                var createMethod = m_Graph.GetType().GetMethod("CreateNode");
                var genericMethod = createMethod.MakeGenericMethod(cn.nodeType);
                var pos = new Vector2(cn.x + offset, cn.y + offset);
                var newNode = (NodeBase)genericMethod.Invoke(m_Graph, new object[] { pos, cn.name, false });
                // CreateNode applies WorldToGridPosition internally — correct to desired grid position
                newNode.SetPosition(new Vector2(cn.x + offset, cn.y + offset));

                // Clear default ports
                newNode.inputPorts.Clear();
                newNode.outputPorts.Clear();

                // Set size
                newNode.SetSize(cn.width, cn.height);

                // Rebuild input ports
                for (int p = 0; p < cn.inputPorts.Count; p++)
                {
                    var cp = cn.inputPorts[p];
                    var port = newNode.AddInputPort(cp.portName, (EdgeMode)cp.edgeMode, cp.canBeDeleted,
                        (EdgeType)cp.edgeType, cp.canBeReordered);
                    portMap[$"{i}:i{p}"] = port;
                }
                // Rebuild output ports
                for (int p = 0; p < cn.outputPorts.Count; p++)
                {
                    var cp = cn.outputPorts[p];
                    var port = newNode.AddOutputPort(cp.portName, (EdgeMode)cp.edgeMode, cp.canBeDeleted,
                        (EdgeType)cp.edgeType, cp.canBeReordered);
                    portMap[$"{i}:o{p}"] = port;
                }

                // Copy data fields
                var fields = newNode.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var f in fields)
                {
                    if (cn.fieldData.TryGetValue(f.Name, out var val))
                        f.SetValue(newNode, val);
                }

                CreateNodeView(newNode);
                newNodes.Add(newNode);
            }

            // Recreate internal edges
            foreach (var ce in s_Clipboard.edges)
            {
                var outKey = $"{ce.outputNodeIndex}:o{ce.outputPortIndex}";
                var inKey = $"{ce.inputNodeIndex}:i{ce.inputPortIndex}";
                if (portMap.TryGetValue(outKey, out var outPort) && portMap.TryGetValue(inKey, out var inPort))
                {
                    m_Graph.CreateEdge(outPort, inPort);
                }
            }

            // Recreate groups
            foreach (var cg in s_Clipboard.groups)
            {
                var newGroup = new NodeGroup();
                newGroup.GenerateNewId();
                newGroup.title = s_Clipboard.pasteCount > 1
                    ? $"{cg.title} ({s_Clipboard.pasteCount})"
                    : cg.title;
                newGroup.isCollapsed = cg.isCollapsed;
                newGroup.x = cg.x + offset;
                newGroup.y = cg.y + offset;
                foreach (var idx in cg.memberIndices)
                {
                    if (idx < newNodes.Count)
                        newGroup.nodeIds.Add(newNodes[idx].id);
                }
                // Remap collapsedPortLabels using oldPortIdMap -> new port id
                foreach (var kvp in cg.collapsedPortLabels)
                {
                    if (cg.oldPortIdMap.TryGetValue(kvp.Key, out var portKey))
                    {
                        if (portMap.TryGetValue(portKey, out var newPort))
                            newGroup.collapsedPortLabels[newPort.id] = kvp.Value;
                    }
                }
                m_Graph.groups.Add(newGroup);
            }

            // Select new nodes
            DeselectAll();
            SelectNodes(newNodes, false);

            // Invalidate caches
            m_Ports = null;
            m_EdgeViews = null;
            m_PointsDirty = true;
            m_Dirty = true;
            Repaint();
            ShowNotification(new GUIContent($"Pasted {newNodes.Count} node(s)"), 1f);
        }

        #endregion

        #region NodeGroups

        private NodeGroup m_CurrentHoveredGroup;

        protected void CreateGroupFromSelection()
        {
            if (m_SelectedNodes == null || m_SelectedNodes.Count < 2) return;
            PushUndoSnapshot();
            var group = new NodeGroup();
            group.GenerateNewId();
            group.title = "Node Group";
            foreach (var node in m_SelectedNodes)
            {
                if (node != null) group.nodeIds.Add(node.id);
            }
            var nodes = new List<NodeBase>(m_SelectedNodes);
            var expandedRect = group.GetExpandedRect(nodes);
            group.x = expandedRect.x;
            group.y = expandedRect.y;
            m_Graph.groups.Add(group);
            DeselectAll();
            m_Dirty = true;
            Repaint();
        }

        protected void DeleteGroup(NodeGroup group)
        {
            if (group == null) return;
            PushUndoSnapshot();
            m_Graph.groups.Remove(group);
            m_Dirty = true;
            Repaint();
        }

        protected void DeleteGroupWithNodes(NodeGroup group)
        {
            if (group == null) return;
            PushUndoSnapshot();
            if (group.nodeIds != null)
            {
                var memberNodes = new List<NodeBase>();
                foreach (var nodeId in group.nodeIds)
                {
                    var node = m_Graph.FindNode(nodeId);
                    if (node != null) memberNodes.Add(node);
                }
                foreach (var node in memberNodes)
                {
                    if (node == null) continue;
                    // Disconnect edges
                    foreach (EdgeView edgeView in edgeViews.Values)
                    {
                        if (edgeView == null) continue;
                        if (edgeView.inputNode == node && edgeView.outputPort != null)
                            edgeView.outputPort.DisconnectFromNode(node.id, m_Graph);
                        if (edgeView.outputNode == node && edgeView.inputPort != null)
                            edgeView.inputPort.DisconnectFromNode(node.id, m_Graph);
                    }
                    // Remove ports without calling PushUndoSnapshot (already done above)
                    for (int i = node.inputPorts.Count - 1; i >= 0; i--)
                    {
                        var port = node.inputPorts[i];
                        if (port != null) { DisconnectPort(port); }
                    }
                    for (int i = node.outputPorts.Count - 1; i >= 0; i--)
                    {
                        var port = node.outputPorts[i];
                        if (port != null) { DisconnectPort(port); }
                    }
                    m_Graph.RemoveNode(node.id);
                    nodeViews.Remove(node.id);
                }
            }
            m_Graph.groups.Remove(group);
            m_Ports = null;
            m_EdgeViews = null;
            m_PointsDirty = true;
            m_Dirty = true;
            Repaint();
        }

        protected void RemoveNodeFromGroup(string nodeId, NodeGroup group)
        {
            if (group == null || !group.nodeIds.Contains(nodeId)) return;
            PushUndoSnapshot();
            group.nodeIds.Remove(nodeId);
            if (group.nodeIds.Count == 0)
                m_Graph.groups.Remove(group);
            m_Dirty = true;
            Repaint();
        }

        protected void AddNodeToGroup(string nodeId, NodeGroup group)
        {
            if (group == null || group.nodeIds.Contains(nodeId)) return;
            PushUndoSnapshot();
            group.nodeIds.Add(nodeId);
            m_Dirty = true;
            Repaint();
        }

        protected void ToggleGroupCollapse(NodeGroup group)
        {
            if (group == null) return;
            if (group.isCollapsed) ExpandGroup(group);
            else
            {
                var nodes = GetGroupNodes(group);
                var rect = group.GetExpandedRect(nodes);
                CollapseGroup(group, rect);
            }
        }

        protected void StartGroupTitleEdit(NodeGroup group)
        {
            if (group == null) return;
            group.isTitleEditing = true;
            group.titleEditBuffer = group.title;
            m_Dirty = true;
            Repaint();
        }

        protected NodeGroup GetGroupAtWorldPosition(Vector2 worldPosition)
        {
            if (m_Graph?.groups == null) return null;
            // Port/group positions are in node-GUI space (grid + panOffset/zoom) = worldPos / zoom
            var nodeGUISpacePos = worldPosition / currentZoom;
            for (int i = m_Graph.groups.Count - 1; i >= 0; i--)
            {
                var group = m_Graph.groups[i];
                if (group == null) continue;
                var nodes = GetGroupNodes(group);
                var rect = group.GetCurrentRect(nodes);
                rect.position += m_Graph.currentPanOffset / currentZoom;
                if (rect.Contains(nodeGUISpacePos))
                    return group;
            }
            return null;
        }

        protected void ShowGroupContextMenu(NodeGroup group)
        {
            var menu = new GenericMenu();
            if (group.isCollapsed)
            {
                menu.AddItem(new GUIContent("Expand"), false, () => ExpandGroup(group));
            }
            else
            {
                menu.AddItem(new GUIContent("Collapse"), false, () =>
                {
                    var nodes = GetGroupNodes(group);
                    var rect = group.GetExpandedRect(nodes);
                    CollapseGroup(group, rect);
                });
            }
            menu.AddItem(new GUIContent("Rename"), false, () => StartGroupTitleEdit(group));
            menu.AddItem(new GUIContent("Ungroup"), false, () => DeleteGroup(group));
            menu.AddItem(new GUIContent("Delete Group and Nodes"), false, () => DeleteGroupWithNodes(group));
            menu.ShowAsContext();
        }

        [NonSerialized] internal static string s_PortRenameId;
        [NonSerialized] internal static string s_PortRenameBuffer;

        protected Port GetCollapsedGroupPortAtPosition(Vector2 worldPosition)
        {
            if (m_Graph?.groups == null) return null;
            // Port positions are in node-GUI space = worldPos / zoom
            var nodeGUISpacePos = worldPosition / currentZoom;
            foreach (var group in m_Graph.groups)
            {
                if (group == null || !group.isCollapsed) continue;
                if (group.collapsedPorts == null) continue;
                foreach (var kvp in group.collapsedPorts)
                {
                    if (group.collapsedPortScreenPos.TryGetValue(kvp.Key, out var pos))
                    {
                        // Check dot rect
                        var portRect = new Rect(pos.x - 8, pos.y - 8, 16, 16);
                        if (portRect.Contains(nodeGUISpacePos))
                            return kvp.Value;
                    }
                    // Check label rect
                    var labelKey = "label_" + kvp.Key;
                    var labelSizeKey = "labelSize_" + kvp.Key;
                    if (group.collapsedPortScreenPos.TryGetValue(labelKey, out var labelPos) &&
                        group.collapsedPortScreenPos.TryGetValue(labelSizeKey, out var labelSize))
                    {
                        var labelRect = new Rect(labelPos.x, labelPos.y, labelSize.x, labelSize.y);
                        if (labelRect.Contains(nodeGUISpacePos))
                            return kvp.Value;
                    }
                }
            }
            return null;
        }

        protected void ShowCollapsedPortContextMenu(Port port, NodeGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Rename Port"), false, () =>
            {
                s_PortRenameId = port.id;
                s_PortRenameBuffer = group.collapsedPortLabels.TryGetValue(port.id, out var lbl) ? lbl : port.portName;
                m_Dirty = true;
                Repaint();
            });
            menu.AddItem(new GUIContent("Reset Name"), false, () =>
            {
                PushUndoSnapshot();
                group.collapsedPortLabels.Remove(port.id);
                m_Dirty = true;
                Repaint();
            });
            menu.ShowAsContext();
        }

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
                    m_Dirty = true;
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

                // Check collapsed group drag
                var clickedGroup = GetGroupAtWorldPosition(current.mousePosition);
                if (clickedGroup != null && clickedGroup.isCollapsed)
                {
                    m_DraggingGroup = clickedGroup;
                    m_SelectedGroup = clickedGroup;
                    m_DragGroupStartPos = new Vector2(clickedGroup.x, clickedGroup.y);
                    // Also prepare to drag member nodes
                    var groupNodes = GetGroupNodes(clickedGroup);
                    m_SelectedNodes.Clear();
                    foreach (var n in groupNodes)
                        m_SelectedNodes.Add(n);
                    UpdateNodesSelectedState(m_SelectedNodes);
                    PrepareToDragSelectedNodes(current.mousePosition);
                    m_Mode = GraphMode.Drag;
                    current.Use();
                    return;
                }

                // Check expanded group drag (click on group background/title, not on a node)
                if (clickedGroup != null && !clickedGroup.isCollapsed && m_CurrentHoveredNode == null)
                {
                    m_SelectedGroup = clickedGroup;
                    var groupNodes = GetGroupNodes(clickedGroup);
                    m_SelectedNodes.Clear();
                    foreach (var n in groupNodes)
                        m_SelectedNodes.Add(n);
                    UpdateNodesSelectedState(m_SelectedNodes);
                    PrepareToDragSelectedNodes(current.mousePosition);
                    m_Mode = GraphMode.Drag;
                    current.Use();
                    return;
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
                        var uc = UColor.GetColor();
                        m_CreateEdgeLineColor = m_ActivePort.IsInput()
                            ? uc.edgeInputColor
                            : uc.edgeOutputColor;
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
                    m_DraggingGroup = null;
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
            PushUndoSnapshot();

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

        private bool m_DragUndoPushed;

        private void PrepareToDragSelectedNodes(Vector2 mousePosition)
        {
            m_DragUndoPushed = false;
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
            // Push undo snapshot on first actual drag movement
            if (!m_DragUndoPushed)
            {
                PushUndoSnapshot();
                m_DragUndoPushed = true;
            }
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

            // Move collapsed group box alongside member nodes
            if (m_DraggingGroup != null)
            {
                m_DraggingGroup.x = m_DragGroupStartPos.x + m_DragNodesDistance.x / currentZoom;
                m_DraggingGroup.y = m_DragGroupStartPos.y + m_DragNodesDistance.y / currentZoom;
            }

            UpdateVirtualPointsIsOccupiedStates();
            m_Dirty = true;
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
            PushUndoSnapshot();
            if (outputPort.OverrideConnection()) DisconnectPort(outputPort);
            if (inputPort.OverrideConnection()) DisconnectPort(inputPort);
            ConnectPorts(m_Graph, outputPort, inputPort);
            m_EdgeViews = null;
            m_Dirty = true;
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
            m_Dirty = true;
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
                    DoCopy();
                    break;
                case GraphAction.Connect:
                    break;
                case GraphAction.DeleteNodes:
                    DeleteNodes(m_SelectedNodes);
                    break;
                case GraphAction.Disconnect:
                    break;
                case GraphAction.Paste:
                    DoPaste();
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
            PushUndoSnapshot();
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
            m_Ports = null;
            m_EdgeViews = null;
            m_PointsDirty = true;
            m_Dirty = true;
            Repaint();
            if (startNode == node)
            {
                if (m_Graph.values.Count > 0)
                {
                    m_Graph.startNodeId = m_Graph.values[0].id;
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
            PushUndoSnapshot();
            var startNode = m_Graph.GetStartNode();
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i] == null) nodes.RemoveAt(i);
            }
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
            m_Ports = null;
            m_EdgeViews = null;
            m_PointsDirty = true;
            m_Dirty = true;
            Repaint();
            if (nodes.Contains(startNode))
            {
                if (m_Graph.values.Count > 0)
                {
                    m_Graph.startNodeId = m_Graph.values[0].id;
                }
                else
                {
                    m_Graph.startNodeId = null;
                }
            }
        }

        protected NodeView CreateNodeWithUndo<T>(System.Func<T> createNodeFunc) where T : NodeBase
        {
            PushUndoSnapshot();
            var node = createNodeFunc();
            return CreateNodeView(node);
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
            m_Ports = null;
            m_EdgeViews = null;
            m_PointsDirty = true;
            m_Dirty = true;
            return nodeView;
        }

        protected virtual Type FallBackNodeViewType()
        {
            return typeof(NodeView);
        }

        protected virtual void RemovePort(Port port,bool force = false)
        {
            if (!force && !m_Graph.FindNode(port.nodeId).CanDeletePort(port)) return;
            PushUndoSnapshot();
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
            m_Points = null;
            m_Ports = null;
            m_EdgeViews = null;
            m_ForceRebuild = true;
            m_UndoStack.Clear();
            m_RedoStack.Clear();
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

            // Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z via KeyUp as fallback
            if (e.type == EventType.KeyUp && (e.control || e.command))
            {
                if (e.keyCode == KeyCode.Z)
                {
                    if (e.shift) PerformRedo();
                    else PerformUndo();
                    e.Use();
                    return;
                }
                if (e.keyCode == KeyCode.Y)
                {
                    PerformRedo();
                    e.Use();
                    return;
                }
            }

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

                default:
                    break;

                case KeyCode.Delete: //Delete
                    if (Event.current.mousePosition.x < position.width - m_NodeInspectorWidth)
                    {
                        if (m_SelectedGroup != null)
                        {
                            var g = m_SelectedGroup;
                            m_SelectedGroup = null;
                            m_DraggingGroup = null;
                            m_Mode = GraphMode.None;
                            // If a drag snapshot was pushed (mouse moved after click), remove it
                            // to avoid an extra undo step between selection and delete
                            if (m_DragUndoPushed && m_UndoStack.Count > 0)
                                m_UndoStack.Pop();
                            m_DragUndoPushed = false;
                            DeselectAll();
                            DeleteGroupWithNodes(g);
                        }
                        else
                        {
                            ExecuteGraphAction(GraphAction.DeleteNodes);
                        }
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