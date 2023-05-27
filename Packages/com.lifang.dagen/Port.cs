using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DaGenGraph
{
    [SerializeField]
    public class Port
    {
        #region Editor

#if UNITY_EDITOR
        [NonSerialized] private AnimBool m_ShowHover;
        public AnimBool showHover => m_ShowHover ?? (m_ShowHover = new AnimBool(false));
#endif

        #endregion
        
        #region Properties

        /// <summary> Returns TRUE if this port can establish multiple edges </summary>
        public bool acceptsMultipleConnections => m_EdgeMode == EdgeMode.Multiple;
        /// <summary> [Editor Only] Returns TRUE if this port can be removed. This is used to make sure important ports cannot be deleted by the developer and break the node settings / graph functionality / user experience </summary>
        public bool canBeDeleted => m_CanBeDeleted;
        /// <summary> [Editor Only] Returns TRUE if this port can be reordered. This is used to prevent special ports from being reordered in the node </summary>
        public bool canBeReordered => m_CanBeReordered;
        /// <summary> [Editor Only] Keeps track of all the Connection Points this port has </summary>
        public List<Vector2> edgePoints
        {
            get
            {
                if (m_EdgePoints==null)
                {
                    m_EdgePoints = new List<Vector2>();
                }
                return m_EdgePoints;
            }
            
            set => m_EdgePoints = value;
        }
        /// <summary> Keeps track of all the Connections this port has </summary>
        public List<Edge> edges
        {
            get => m_Edges ?? (m_Edges = new List<Edge>());
            set => m_Edges = value;
        }

        /// <summary> [Editor Only] Returns the curve modifier for this port. Editor option to adjust the edges curve strength </summary>
        public float curveModifier
        {
            get => m_CurveModifier;
            set => m_CurveModifier = value;
        }

        /// <summary> Returns the first Connection this port has. Returns null if no edge exists </summary>
        public Edge firstConnection => edges.Count > 0 ? edges[0] : null;

        /// <summary> [Editor Only] Overlay image Rect that is drawn on mouse hover. Also used to calculate if to show the port context menu </summary>
        [field: NonSerialized]
        public Rect hoverRect { get; set; }

        /// <summary> Returns this port's id </summary>
        public string id
        {
            get => m_ID;
            set => m_ID = value;
        }

        /// <summary> Returns TRUE if this port has at least one edge (it checks the Connections count) </summary>
        public bool isConnected => m_Edges.Count > 0;

        /// <summary> Returns TRUE if this is an Input port </summary>
        public bool isInput => m_Direction == PortDirection.Input;

        /// <summary> Returns TRUE if this is an Output port </summary>
        public bool isOutput => m_Direction == PortDirection.Output;

        /// <summary> Returns TRUE if this port can establish only ONE edge </summary>
        public bool overrideConnection => m_EdgeMode == EdgeMode.Override;

        /// <summary> Returns this port's parent node id </summary>
        public string nodeId
        {
            get => m_NodeId;
            set => m_NodeId = value;
        }

        /// <summary> Returns this port's name </summary>
        public string portName => m_PortName;

        /// <summary> Returns the port's value as a string </summary>
        public string value
        {
            get => m_Value;
            set => m_Value = value;
        }

        #endregion

        #region Private Variables
        
        [SerializeField] private List<Edge> m_Edges;
        [SerializeField] private List<Vector2> m_EdgePoints;
        [SerializeField] private PortDirection m_Direction;
        [SerializeField] private EdgeMode m_EdgeMode;
        [SerializeField] private bool m_CanBeDeleted;
        [SerializeField] private bool m_CanBeReordered;
        [SerializeField] private float m_CurveModifier;
        [SerializeField] private float m_Height;
        [SerializeField] private float m_Width;
        [SerializeField] private float m_X;
        [SerializeField] private float m_Y;
        [SerializeField] private string m_ID;
        [SerializeField] private string m_NodeId;
        [SerializeField] private string m_PortName;
        [SerializeField] private string m_Value;
        
        #endregion

        #region public Methods

        public Port(Node node,string guid, string portName, PortDirection direction, EdgeMode edgeMode,List<Vector2> edgePoints, bool canBeDeleted, bool canBeReordered)
        {
            if (string.IsNullOrEmpty(guid))
            {
                GenerateNewId();
            }
            else
            {
                id=guid;
            }
            m_NodeId = node.id;
            m_PortName = portName;
            m_Direction = direction;
            m_EdgeMode = edgeMode;
            m_EdgePoints = edgePoints;
            m_CanBeDeleted = canBeDeleted;
            m_CanBeReordered = canBeReordered;
            m_Edges = new List<Edge>();
            m_CurveModifier = 0;
        }

        /// <summary> [Editor Only] Returns the height of this port </summary>
        public float GetHeight()
        {
            return m_Height;
        }
        /// <summary> [Editor Only] Returns the position of this port </summary>
        public Vector2 GetPosition()
        {
            return new Vector2(m_X, m_Y);
        }
        /// <summary> [Editor Only] Returns the Rect of this port </summary>
        public Rect GetRect()
        {
            return new Rect(m_X, m_Y, m_Width, m_Height);
        }
        /// <summary> [Editor Only] Returns the size of this port (x is width, y is height) </summary>
        public Vector2 GetSize()
        {
            return new Vector2(m_Width, m_Height);
        }
        /// <summary> [Editor Only] Returns the width of this port </summary>
        public float GetWidth()
        {
            return m_Width;
        }
        /// <summary> [Editor Only] Returns the x coordinate of this port </summary>
        public float GetX()
        {
            return m_X;
        }
        /// <summary> [Editor Only] Returns the y coordinate of this port </summary>
        public float GetY()
        {
            return m_Y;
        }
        /// <summary>
        ///  Returns the edge mode this port has (Multiple/Override)
        ///  <para />
        ///  The edge mode determines if this port can establish multiple edge or just one
        /// </summary>
        public EdgeMode GetConnectionMode()
        {
            return m_EdgeMode;
        }
        /// <summary> Returns the direction this port has (Input/Output) </summary>
        public PortDirection GetDirection()
        {
            return m_Direction;
        }
        
        /// <summary> Returns a IEnumerable of all the Edge ids of this Port </summary>
        public IEnumerable<string> GetEdgeIds()
        {
            return edges.Select(edge => edge.id).ToList();
        }
        /// <summary> Returns the Connection with the given id. If not found it will return null </summary>
        /// <param name="edgenId"> The connection id to look for and return its reference </param>
        public Edge GetEdge(string edgenId)
        {
            return edges.FirstOrDefault(edge => edge.id == edgenId);
        }
        /// <summary> [Editor Only] Sets the height of this port's Rect </summary>
        /// <param name="value"> The new height value </param>
        public void SetHeight(float value)
        {
            m_Height = value;
        }
        /// <summary> Set the name for this port </summary>
        /// <param name="value"> The new port name value </param>
        public void SetName(string value)
        {
            m_PortName = value;
        }
        /// <summary> [Editor Only] Sets the position of this port's Rect </summary>
        /// <param name="position"> The new position value </param>
        public void SetPosition(Vector2 position)
        {
            m_X = position.x;
            m_Y = position.y;
        }
        /// <summary> [Editor Only] Sets the position of this port's Rect </summary>
        /// <param name="x"> The new x coordinate value </param>
        /// <param name="y"> The new y coordinate value </param>
        public void SetPosition(float x, float y)
        {
            m_X = x;
            m_Y = y;
        }
        /// <summary> [Editor Only] Sets the Rect value for this port </summary>
        /// <param name="rect"> The new rect values </param>
        public void SetRect(Rect rect)
        {
            m_X = rect.x;
            m_Y = rect.y;
            m_Width = rect.width;
            m_Height = rect.height;
        }
        /// <summary> [Editor Only] Sets the Rect values for this port </summary>
        /// <param name="position"> The new position value </param>
        /// <param name="size"> The new size value </param>
        public void SetRect(Vector2 position, Vector2 size)
        {
            m_X = position.x;
            m_Y = position.y;
            m_Width = size.x;
            m_Height = size.y;
        }
        /// <summary> [Editor Only] Sets the Rect values for this port </summary>
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
        /// <summary> [Editor Only] Sets the size of this port's Rect </summary>
        /// <param name="size"> The new port size (x is width, y is height) </param>
        public void SetSize(Vector2 size)
        {
            m_Width = size.x;
            m_Height = size.y;
        }
        /// <summary> [Editor Only] Sets the size of this port's Rect </summary>
        /// <param name="width"> The new width value </param>
        /// <param name="height"> The new height value </param>
        public void SetSize(float width, float height)
        {
            m_Width = width;
            m_Height = height;
        }
        /// <summary> [Editor Only] Sets the width of this port's Rect </summary>
        /// <param name="value"> The new width value </param>
        public void SetWidth(float value)
        {
            m_Width = value;
        }
        /// <summary> [Editor Only] Sets the x coordinate of this port's Rect </summary>
        /// <param name="value"> The new x value </param>
        public void SetX(float value)
        {
            m_X = value;
        }
        /// <summary> [Editor Only] Sets the y coordinate of this port's Rect </summary>
        /// <param name="value"> The new y value </param>
        public void SetY(float value)
        {
            m_Y = value;
        }
        /// <summary> [Editor Only] Updates the port hover Rect. This is the 'selection' box that appears when the mouse is over the port </summary>
        public void UpdateHoverRect()
        {
            hoverRect = new Rect(GetRect().x + 6, 
                GetRect().y + 2,
                GetRect().width - 12,
                GetRect().height - 3);
        }
        /// <summary> Returns TRUE if this port can connect to another port </summary>
        /// <param name="other"> The other port we are trying to determine if this port can connect to </param>
        /// <param name="ignoreValueType"> If true, this check will not make sure that the sockets valueTypes match </param>
        public bool CanConnect(Port other, bool ignoreValueType = false)
        {
            if (other == null) return false; //check that the other port is not null
            if (IsConnectedToPort(other.id))return false; //check that this port is not already connected to the other port
            if (id == other.id) return false; //make sure we're not trying to connect the same port
            if (nodeId == other.nodeId) return false; //check that we are not connecting sockets on the same baseNode
            if (isInput && other.isInput) return false; //check that the sockets are not both input sockets
            if (isOutput && other.isOutput) return false; //check that the sockets are not both output sockets
            return true;
        }
        
        /// <summary> Removes a edge with the given edge id from this Port </summary>
        /// <param name="edgeId"> The edge id we want removed from this Port </param>
        public void RemoveEdge(string edgeId)
        {
            //if the connections list does not contain this connection id -> return;
            if (!ContainsEdge(edgeId)) return; 
            //iterate through all the connections list
            for (var i = edges.Count - 1; i >= 0; i--)
            {
                //if a connection has the given connection id -> remove connection
                if (edges[i].id == edgeId)
                {
                    edges.RemoveAt(i);
                }
            }
        }

        /// <summary> Returns TRUE if this port contains a edge with the given edge id </summary>
        /// <param name="edgeId"> The edge id to search for </param>
        public bool ContainsEdge(string edgeId)
        {
            return edges.Any(edge => edge.id == edgeId);
        }

        /// <summary> [Editor Only] Returns the closest own Edge point to the closest Edge point on the other Port </summary>
        public Vector2 GetClosestEdgePointToPort(Port other)
        {
            //arbitrary value that will surely be greater than any other possible distance
            float minDistance = 100000; 
            //set the closest point as the first connection point
            var closestPoint = edgePoints[0];
            //iterate through this port's own connection points list
            foreach (var ownPoint in edgePoints)
            {
                foreach (var distance in other.edgePoints.Select(otherPoint => Vector2.Distance(ownPoint, otherPoint)).Where(distance => !(distance > minDistance)))
                {
                    //the distance is smaller than the current minimum distance -> update the selected connection point
                    closestPoint =ownPoint; 
                    //update the current minimum distance
                    minDistance = distance;
                }
            }
            return closestPoint; //return the closest Edge point
        }

        /// <summary> Remove ALL the Edges this Port has, by clearing the Edges list </summary>
        public void Disconnect()
        {
            edges.Clear();
        }
        
        /// <summary> Disconnect this port from the given node id </summary>
        /// <param name="nodeId"> The node id we want this port to disconnect from </param>
        public void DisconnectFromNode(string nodeId)
        {
            if (!isConnected) return;
            for (int i = edges.Count - 1; i >= 0; i--) 
            {
                var edge = edges[i];
                if (isInput && edge.outputNodeId == nodeId) edges.RemoveAt(i); 
                if (isOutput && edge.inputNodeId == nodeId) edges.RemoveAt(i);
            }
        }
        
        #endregion

        #region private Methods

        /// <summary> Returns TRUE if this port is connected to the given port id </summary>
        /// <param name="portId"> The port id to search for and determine if this port is connected to or not </param>
        private bool IsConnectedToPort(string portId)
        {
            foreach (var connection in edges) //iterate through all the connections list
            {
                if (isInput && connection.outputPortId == portId)return true; //if this is an input port -> look for the port id at the output port of the connection
                if (isOutput && connection.inputPortId == portId)return true; //if this is an output port -> look for the port id at the input port of the connection
            }
            return false;
        }
        
        /// <summary> Generates a new unique port id for this port and returns the newly generated id value </summary>
        private string GenerateNewId()
        {
            m_ID = Guid.NewGuid().ToString();
            return m_ID;
        }
        
        #endregion
    }
    public enum PortDirection
    {
        /// <summary>
        ///     An input Socket can only connect to an output Socket
        /// </summary>
        Input,

        /// <summary>
        ///     An output Socket can only connect to an input Socket
        /// </summary>
        Output
    }
    public enum EdgeMode
    {
        /// <summary>
        ///    Socket can have only one Connection at a time (overriding any existing edge upon establishing a new edge)
        /// </summary>
        Override,

        /// <summary>
        ///     Socket can have multiple edges
        /// </summary>
        Multiple
    }
}