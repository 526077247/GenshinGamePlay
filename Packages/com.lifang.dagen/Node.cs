using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DaGenGraph
{
    public class Node : SerializedScriptableObject
    {
        #region Private Variables

        [SerializeField, HideInInspector] private List<Port> m_InputPorts;
        [SerializeField, HideInInspector] private List<Port> m_OutputPorts;
        [SerializeField, HideInInspector] private bool m_AllowDuplicateNodeName;
        [SerializeField, HideInInspector] private bool m_AllowEmptyNodeName;
        [SerializeField, HideInInspector] private bool m_CanBeDeleted;
        [SerializeField, HideInInspector] private float m_Height;
        [SerializeField, HideInInspector] private float m_Width;
        [SerializeField, HideInInspector] private float m_X;
        [SerializeField, HideInInspector] private float m_Y;
        [SerializeField, HideInInspector] private int m_MinimumInputPortsCount;
        [SerializeField, HideInInspector] private int m_MinimumOutputPortsCount;
        [SerializeField, HideInInspector] private string m_GraphId;
        [SerializeField, HideInInspector] private string m_Id;
        [NonSerialized] private Graph m_ActiveGraph;

        #endregion

        #region public Variables

        [HideInInspector] public bool isHovered;
        [HideInInspector] public bool errorNodeNameIsEmpty;
        [HideInInspector] public bool errorDuplicateNameFoundInGraph;
        [HideInInspector] public DeletePort deletePort;

        [HideInInspector]
        public delegate void DeletePort(Port port);

        #endregion

        #region Properties

        public virtual bool hasErrors => errorNodeNameIsEmpty || errorDuplicateNameFoundInGraph;

        /// <summary> Returns TRUE if this node can have an empty node name </summary>
        private bool allowEmptyNodeName => m_AllowEmptyNodeName;

        /// <summary> Returns TRUE if this node can have the same name as another node </summary>
        public bool allowDuplicateNodeName => m_AllowDuplicateNodeName;

        /// <summary> Returns TRUE if this can be deleted </summary>
        public bool canBeDeleted
        {
            get => m_CanBeDeleted;
            set => m_CanBeDeleted = value;
        }

        /// <summary> Trigger a visual cue for this node, in the Editor, at runtime. Mostly used when this node has been activated </summary>
        public bool ping { get; set; }
        

        /// <summary> Returns a reference to the currently active graph </summary>
        public Graph activeGraph
        {
            get => m_ActiveGraph;
            set => m_ActiveGraph = value;
        }

        /// <summary> The minimum number of input ports for this node. This value is checked when deleting input ports </summary>
        public int minimumInputPortsCount
        {
            get => m_MinimumInputPortsCount;
            set => m_MinimumInputPortsCount = value;
        }

        /// <summary> The minimum number of output ports for this node. This value is checked when deleting output ports </summary>
        public int minimumOutputPortsCount
        {
            get => m_MinimumOutputPortsCount;
            set => m_MinimumOutputPortsCount = value;
        }

        /// <summary> List of all the input ports this node has </summary>
        public List<Port> inputPorts
        {
            get => m_InputPorts ?? (m_InputPorts = new List<Port>());
            set => m_InputPorts = value;
        }

        /// <summary> List of all the output ports this node has </summary>
        public List<Port> outputPorts
        {
            get => m_OutputPorts ?? (m_OutputPorts = new List<Port>());
            set => m_OutputPorts = value;
        }

        /// <summary> Returns the first input port. If there isn't one, it returns null </summary>
        public Port firstInputPort => inputPorts.Count > 0 ? inputPorts[0] : null;

        /// <summary> Returns the first output port. If there isn't one, it returns null </summary>
        public Port firstOutputPort => outputPorts.Count > 0 ? outputPorts[0] : null;

        /// <summary> Returns this node's parent graph id </summary>
        public string graphId
        {
            get => m_GraphId;
            set => m_GraphId = value;
        }

        /// <summary> Returns this node's id </summary>
        public string id
        {
            get
            {
                if (string.IsNullOrEmpty(m_Id))
                {
                    m_Id = Guid.NewGuid().ToString();
                }

                return m_Id;
            }
        }
        

        /// <summary> Returns this node's outputNodeIds </summary>
        public List<string> outputNodeIds
        {
            get
            {
                var nodes = new List<string>();
                foreach (var port in m_OutputPorts)
                {
                    foreach (var edge in port.edges)
                    {
                        nodes.Add(edge.outputNodeId);
                    }
                }

                return nodes;
            }
        }

        /// <summary> Returns this node's intputNodeIds </summary>
        public List<string> intputNodeIds
        {
            get
            {
                var nodes = new List<string>();
                foreach (var port in m_InputPorts)
                {
                    foreach (var edge in port.edges)
                    {
                        nodes.Add(edge.inputNodeId);
                    }
                }

                return nodes;
            }
        }

        #endregion

        #region Editor

        private void CheckThatNodeNameIsNotEmpty()
        {
#if UNITY_EDITOR
            errorNodeNameIsEmpty = false;
            if (allowEmptyNodeName) return;
            errorNodeNameIsEmpty = string.IsNullOrEmpty(name.Trim());
#endif
        }

        /// <summary>
        ///     Checks if this node has any errors. Because each type of node can have different errors, this method is used to define said custom errors and reflect that in the NodeGraph (for the NodeGUI) and in the Inspector (for
        ///     the NodeEditor)
        /// </summary>
        public virtual void CheckForErrors()
        {
#if UNITY_EDITOR
            CheckThatNodeNameIsNotEmpty();
#endif
        }

        #endregion

        #region Protected  Methods

        protected virtual void OnEnable()
        {
        }

        /// <summary> Set to allow this node to have an empty node name </summary>
        /// <param name="value"> Disable error for empty node name </param>
        protected void SetAllowEmptyNodeName(bool value)
        {
            m_AllowEmptyNodeName = value;
        }

        /// <summary> Set to allow this node to have a duplicate node name </summary>
        /// <param name="value"> Disable error for duplicate node name </param>
        protected void SetAllowDuplicateNodeName(bool value)
        {
            m_AllowDuplicateNodeName = value;
        }


        /// <summary> OnEnterNode is called on the frame when this node becomes active just before any of the node's Update methods are called for the first time </summary>
        /// <param name="previousActiveNode"> The node that was active before this one </param>
        /// <param name="edge"> The edge that activated this node </param>
        public virtual void OnEnter(Node previousActiveNode, Edge edge)
        {
            ping = true;
        }

        /// <summary> OnExitNode is called just before this node becomes inactive </summary>
        /// <param name="nextActiveNode"> The node that will become active next</param>
        /// <param name="edge"> The edge that activates the next node </param>
        public virtual void OnExit(Node nextActiveNode, Edge edge)
        {
            ping = false;
            if (edge != null)
            {
                edge.ping = true;
                edge.reSetTime = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary> Set the active Graph for this node </summary>
        /// <param name="graph"> Target Graph </param>
        public void SetActiveGraph(Graph graph)
        {
            activeGraph = graph;
        }
        
        /// <summary> Returns the x coordinate of this node </summary>
        public float GetX()
        {
            return m_X;
        }

        /// <summary> Returns the y coordinate of this node </summary>
        public float GetY()
        {
            return m_Y;
        }

        /// <summary> Returns the width of this node </summary>
        public float GetWidth()
        {
            return m_Width;
        }

        /// <summary> Returns the height of this node </summary>
        public float GetHeight()
        {
            return m_Height;
        }

        /// <summary> Returns the position of this node </summary>
        public Vector2 GetPosition()
        {
            return new Vector2(m_X, m_Y);
        }

        /// <summary> Returns the size of this node (x is width, y is height) </summary>
        public Vector2 GetSize()
        {
            return new Vector2(GetWidth(), m_Height);
        }

        /// <summary> Returns the Rect of this node </summary>
        public Rect GetRect()
        {
            return new Rect(m_X, m_Y, GetWidth(), m_Height);
        }

        public Rect GetFooterRect()
        {
            return new Rect(GetX() + 6, GetY() - 6 + GetHeight() - 10, GetWidth() - 12, 10);
        }

        /// <summary> Returns the Rect of this node's header </summary>
        public Rect GetHeaderRect()
        {
            return new Rect(GetX() + 6, GetY() + 6, GetWidth() - 12, 32);
        }

        /// <summary> Set the name for this node </summary>
        /// <param name="value"> The new node name value </param>
        public void SetName(string value)
        {
            name = value;
        }

        /// <summary> Set the position of this node's Rect </summary>
        /// <param name="position"> The new position value </param>
        public void SetPosition(Vector2 position)
        {
            m_X = position.x;
            m_Y = position.y;
        }

        /// <summary> Set the position of this node's Rect </summary>
        /// <param name="x"> The new x coordinate value </param>
        /// <param name="y"> The new y coordinate value </param>
        public void SetPosition(float x, float y)
        {
            m_X = x;
            m_Y = y;
        }

        /// <summary> Set the Rect values for this node </summary>
        /// <param name="rect"> The new rect values </param>
        public void SetRect(Rect rect)
        {
            m_X = rect.x;
            m_Y = rect.y;
            m_Width = rect.width;
            m_Height = rect.height;
        }

        /// <summary> Set the Rect values for this node </summary>
        /// <param name="position"> The new position value </param>
        /// <param name="size"> The new size value </param>
        public void SetRect(Vector2 position, Vector2 size)
        {
            m_X = position.x;
            m_Y = position.y;
            m_Width = size.x;
            m_Height = size.y;
        }

        /// <summary> Set the Rect values for this node </summary>
        /// <param name="x"> The new x coordinate value </param>
        /// <param name="y"> The new y coordinate value </param>
        /// <param name="width"> The new width value </param>
        /// <param name="height"> The new height value </param>
        public void SetRect(float x, float y, float width, float height)
        {
            m_X = x;
            m_X = y;
            m_Width = width;
            m_Height = height;
        }

        /// <summary> Set the size of this node's Rect </summary>
        /// <param name="size"> The new node size (x is width, y is height) </param>
        public void SetSize(Vector2 size)
        {
            m_Width = size.x;
            m_Height = size.y;
        }

        /// <summary> Set the size of this node's Rect </summary>
        /// <param name="width"> The new width value </param>
        /// <param name="height"> The new height value </param>
        public void SetSize(float width, float height)
        {
            m_Width = width;
            m_Height = height;
        }

        /// <summary> Set the width of this node's Rect </summary>
        /// <param name="value"> The new width value </param>
        public void SetWidth(float value)
        {
            m_Width = value;
        }

        /// <summary> Set the height of this node's Rect </summary>
        /// <param name="value"> The new height value </param>
        public void SetHeight(float value)
        {
            m_Height = value;
        }

        /// <summary> Set the x coordinate of this node's Rect </summary>
        /// <param name="value"> The new x value </param>
        public void SetX(float value)
        {
            m_X = value;
        }

        /// <summary> Set the y coordinate of this node's Rect </summary>
        /// <param name="value"> The new y value </param>
        public void SetY(float value)
        {
            m_Y = value;
        }

        /// <summary> Convenience method to add a new input port to this node </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="edgePoints"> The port edge points locations (if null or empty, it will automatically add two edge points to the left of and the right of the port) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddInputPort(string portName, EdgeMode edgeMode, List<Vector2> edgePoints, bool canBeDeleted,
            bool canBeReordered = true)
        {
            return AddPort(portName, PortDirection.Input, edgeMode, edgePoints, canBeDeleted, canBeReordered);
        }

        /// <summary> Convenience method to add a new input port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddInputPort(string portName, EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort(portName, PortDirection.Input, edgeMode, GetLeftAndRightEdgePoints(), canBeDeleted,
                canBeReordered);
        }

        /// <summary> Convenience method to add a new input port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddInputPort(EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort("", PortDirection.Input, edgeMode, GetLeftAndRightEdgePoints(), canBeDeleted,
                canBeReordered);
        }

        /// <summary> Convenience method to add a new output port to this node </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="edgePoints"> The port edge points locations (if null or empty, it will automatically add two edge points to the left of and the right of the port) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddOutputPort(string portName, EdgeMode edgeMode, List<Vector2> edgePoints, bool canBeDeleted,
            bool canBeReordered)
        {
            return AddPort(portName, PortDirection.Output, edgeMode, edgePoints, canBeDeleted, canBeReordered);
        }

        /// <summary> Convenience method to add a new output port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddOutputPort(string portName, EdgeMode edgeMode, bool canBeDeleted,
            bool canBeReordered)
        {
            return AddPort(portName, PortDirection.Output, edgeMode, GetLeftAndRightEdgePoints(), canBeDeleted,
                canBeReordered);
        }

        /// <summary> Convenience method to add a new output port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddOutputPort(EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort("", PortDirection.Output, edgeMode, GetLeftAndRightEdgePoints(), canBeDeleted,
                canBeReordered);
        }

        /// <summary> Returns TRUE if the target port can be deleted, after checking is it is marked as 'deletable' and that by deleting it the node minimum ports count does not go below the set threshold </summary>
        /// <param name="port">Target port</param>
        public bool CanDeletePort(Port port)
        {
            //if port is market as cannot be deleted -> return false -> do not allow the dev to delete this port
            if (!port.canBeDeleted) return false;
            //if port is input -> make sure the node has a minimum input ports count before allowing deletion
            if (port.isInput) return inputPorts.Count > m_MinimumInputPortsCount;
            //if port is output -> make sure the node has a minimum output ports count before allowing deletion
            if (port.isOutput) return outputPorts.Count > m_MinimumOutputPortsCount;
            //event though the port can be deleted -> the node needs to hold a minimum number of ports and will not allow to delete this port
            return false;
        }

        /// <summary> Returns TRUE if a edge with the given id can be found on one of this node's ports </summary>
        /// <param name="edgeId"> Target edge id </param>
        public bool ContainsEdge(string edgeId)
        {
            return GetEdge(edgeId) != null;
        }

        /// <summary> Returns a edge, from this node, with the matching edge id. Returns null if no edge with the given id is found </summary>
        /// <param name="edgeId"> Target edge id </param>
        public Edge GetEdge(string edgeId)
        {
            Edge edge;
            foreach (var port in inputPorts)
            {
                edge = port.GetEdge(edgeId);
                if (edge != null) return edge;
            }

            foreach (var port in outputPorts)
            {
                edge = port.GetEdge(edgeId);
                if (edge != null) return edge;
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary> Adds a port to this node </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="direction"> The port direction (Input/Output) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="edgePoints"> The port edge points locations (if null or empty, it will automatically add two edge points to the left of and the right of the port) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        private Port AddPort(string portName, PortDirection direction, EdgeMode edgeMode, List<Vector2> edgePoints,
            bool canBeDeleted, bool canBeReordered)
        {
            if (edgePoints == null)
            {
                edgePoints = new List<Vector2>(GetLeftAndRightEdgePoints());
            }

            if (edgePoints.Count == 0)
            {
                edgePoints.AddRange(GetLeftAndRightEdgePoints());
            }

            var portNames = new List<string>();
            int counter;
            switch (direction)
            {
                case PortDirection.Input:
                    foreach (Port port in m_InputPorts)
                        portNames.Add(port.portName);
                    counter = 0;
                    if (string.IsNullOrEmpty(portName))
                    {
                        portName = "InputPort_" + counter;
                    }

                    while (portNames.Contains(portName))
                    {
                        portName = "InputPort_" + counter++;
                    }

                    var inputPort = new Port(this, id, portName, direction, edgeMode, edgePoints, canBeDeleted,
                        canBeReordered);
                    m_InputPorts.Add(inputPort);
                    return inputPort;
                case PortDirection.Output:
                    foreach (Port port in m_OutputPorts)
                        portNames.Add(port.portName);
                    counter = 0;
                    if (string.IsNullOrEmpty(portName))
                    {
                        portName = "OutputPort_" + counter;
                    }

                    while (portNames.Contains(portName))
                    {
                        portName = "OutputPort_" + counter++;
                    }

                    var outputPort = new Port(this, "", portName, direction, edgeMode, edgePoints, canBeDeleted,
                        canBeReordered);
                    m_OutputPorts.Add(outputPort);
                    return outputPort;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary> Returns a list of two Edge points positions to the left of and the right of the Port </summary>
        private List<Vector2> GetLeftAndRightEdgePoints()
        {
            return new List<Vector2> {GetLeftEdgePointPosition(), GetRightEdgePointPosition()};
        }

        /// <summary> Returns the default left edge point position for a Port </summary>
        private Vector2 GetLeftEdgePointPosition()
        {
            return new Vector2(-2f, 24f / 2 - 16f / 2);
        }

        /// <summary> Returns the default right edge point position for a Port </summary>
        private Vector2 GetRightEdgePointPosition()
        {
            return new Vector2(GetWidth() + 2f - 16f, 24f / 2 - 16f / 2);
        }

        /// <summary> Generates a new unique node id for this node and returns the newly generated id value </summary>
        private void GenerateNewId()
        {
            m_Id = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public virtual Methods

        public virtual void InitNode(Graph graph, Vector2 pos, string _name, int minimumInputPortsCount = 1,
            int minimumOutputPortsCount = 0)
        {
            name = _name;
            GenerateNewId();
            m_GraphId = graph.guid;
            m_InputPorts = new List<Port>();
            m_OutputPorts = new List<Port>();
            m_CanBeDeleted = true;
            m_MinimumInputPortsCount = minimumInputPortsCount;
            m_MinimumOutputPortsCount = minimumOutputPortsCount;
            m_X = pos.x;
            m_Y = pos.y;
            m_Width = 216f;
            m_Height = 216f;
        }

        public virtual void AddDefaultPorts()
        {
            AddInputPort(EdgeMode.Multiple, false, false);
            //AddOutputPort(EdgeMode.Override, true, true);
        }

        #endregion
    }
}