using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DaGenGraph
{
    [PortGroup(0)]
    public class Port: ScriptableObject
    {
        #region Editor

#if UNITY_EDITOR
        [NonSerialized] private AnimBool m_ShowHover;
        public AnimBool showHover => m_ShowHover ?? new AnimBool(false);
#endif

        #endregion
        
        #region Properties

        /// <summary> Returns TRUE if this port can establish multiple edges </summary>
        public bool acceptsMultipleConnections => edgeMode == EdgeMode.Multiple;
        

        /// <summary> [Editor Only] Overlay image Rect that is drawn on mouse hover. Also used to calculate if to show the port context menu </summary>
        public Rect hoverRect { get; set; }

        /// <summary> Returns TRUE if this port has at least one edge (it checks the Connections count) </summary>
        public bool isConnected => edges.Count > 0;

        /// <summary> Returns TRUE if this is an Input port </summary>
        public bool isInput => direction == PortDirection.Input;

        /// <summary> Returns TRUE if this is an Output port </summary>
        public bool isOutput => direction == PortDirection.Output;

        /// <summary> Returns TRUE if this port can establish only ONE edge </summary>
        public bool overrideConnection => edgeMode == EdgeMode.Override;
        
        #endregion

        #region public Variables
        
        public List<string> edges;
        public List<Vector2> edgePoints;
        public PortDirection direction;
        public EdgeMode edgeMode;
        public bool canBeDeleted;
        public bool canBeReordered;
        public float curveModifier;
        public float height;
        public float width;
        public float x;
        public float y;
        public string id;
        public string nodeId;
        public string portName;

        #endregion

        #region public Methods
        
        public void Init(NodeBase node, string portName, PortDirection direction, EdgeMode edgeMode,List<Vector2> edgePoints, bool canBeDeleted, bool canBeReordered)
        {
            GenerateNewId();
            nodeId = node.id;
            this.portName = portName;
            this.direction = direction;
            this.edgeMode = edgeMode;
            this.edgePoints = edgePoints;
            this.canBeDeleted = canBeDeleted;
            this.canBeReordered = canBeReordered;
            edges = new List<string>();
            curveModifier = 0;
        }

        /// <summary> [Editor Only] Returns the height of this port </summary>
        public float GetHeight()
        {
            return height;
        }
        /// <summary> [Editor Only] Returns the position of this port </summary>
        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }
        /// <summary> [Editor Only] Returns the Rect of this port </summary>
        public Rect GetRect()
        {
            return new Rect(x, y, width, height);
        }
        /// <summary> [Editor Only] Returns the size of this port (x is width, y is height) </summary>
        public Vector2 GetSize()
        {
            return new Vector2(width, height);
        }
        /// <summary> [Editor Only] Returns the width of this port </summary>
        public float GetWidth()
        {
            return width;
        }
        /// <summary> [Editor Only] Returns the x coordinate of this port </summary>
        public float GetX()
        {
            return x;
        }
        /// <summary> [Editor Only] Returns the y coordinate of this port </summary>
        public float GetY()
        {
            return y;
        }
        /// <summary>
        ///  Returns the edge mode this port has (Multiple/Override)
        ///  <para />
        ///  The edge mode determines if this port can establish multiple edge or just one
        /// </summary>
        public EdgeMode GetConnectionMode()
        {
            return edgeMode;
        }
        /// <summary> Returns the direction this port has (Input/Output) </summary>
        public PortDirection GetDirection()
        {
            return direction;
        }
        
        /// <summary> Returns a IEnumerable of all the Edge ids of this Port </summary>
        public IEnumerable<string> GetEdgeIds()
        {
            return edges.ToList();
        }

        /// <summary> [Editor Only] Sets the height of this port's Rect </summary>
        /// <param name="value"> The new height value </param>
        public void SetHeight(float value)
        {
            height = value;
        }
        /// <summary> Set the name for this port </summary>
        /// <param name="value"> The new port name value </param>
        public void SetName(string value)
        {
            portName = value;
        }
        /// <summary> [Editor Only] Sets the position of this port's Rect </summary>
        /// <param name="position"> The new position value </param>
        public void SetPosition(Vector2 position)
        {
            x = position.x;
            y = position.y;
        }
        /// <summary> [Editor Only] Sets the position of this port's Rect </summary>
        /// <param name="x"> The new x coordinate value </param>
        /// <param name="y"> The new y coordinate value </param>
        public void SetPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        /// <summary> [Editor Only] Sets the Rect value for this port </summary>
        /// <param name="rect"> The new rect values </param>
        public void SetRect(Rect rect)
        {
            x = rect.x;
            y = rect.y;
            width = rect.width;
            height = rect.height;
        }
        /// <summary> [Editor Only] Sets the Rect values for this port </summary>
        /// <param name="position"> The new position value </param>
        /// <param name="size"> The new size value </param>
        public void SetRect(Vector2 position, Vector2 size)
        {
            x = position.x;
            y = position.y;
            width = size.x;
            height = size.y;
        }
        /// <summary> [Editor Only] Sets the Rect values for this port </summary>
        /// <param name="x"> The new x coordinate value </param>
        /// <param name="y"> The new y coordinate value </param>
        /// <param name="width"> The new width value </param>
        /// <param name="height"> The new height value </param>
        public void SetRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.x = y;
            this.width = width;
            this.height = height;
        }
        /// <summary> [Editor Only] Sets the size of this port's Rect </summary>
        /// <param name="size"> The new port size (x is width, y is height) </param>
        public void SetSize(Vector2 size)
        {
            width = size.x;
            height = size.y;
        }
        /// <summary> [Editor Only] Sets the size of this port's Rect </summary>
        /// <param name="width"> The new width value </param>
        /// <param name="height"> The new height value </param>
        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
        /// <summary> [Editor Only] Sets the width of this port's Rect </summary>
        /// <param name="value"> The new width value </param>
        public void SetWidth(float value)
        {
            width = value;
        }
        /// <summary> [Editor Only] Sets the x coordinate of this port's Rect </summary>
        /// <param name="value"> The new x value </param>
        public void SetX(float value)
        {
            x = value;
        }
        /// <summary> [Editor Only] Sets the y coordinate of this port's Rect </summary>
        /// <param name="value"> The new y value </param>
        public void SetY(float value)
        {
            y = value;
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
        public bool CanConnect(Port other,GraphBase graphBase, bool ignoreValueType = false)
        {
            if (other == null) return false; //check that the other port is not null
            if (IsConnectedToPort(other.id,graphBase))return false; //check that this port is not already connected to the other port
            if (id == other.id) return false; //make sure we're not trying to connect the same port
            if (nodeId == other.nodeId) return false; //check that we are not connecting sockets on the same baseNode
            if (isInput && other.isInput) return false; //check that the sockets are not both input sockets
            if (isOutput && other.isOutput) return false; //check that the sockets are not both output sockets
            if (!ignoreValueType)
            {
                var attributes1 = GetType().GetCustomAttributes(typeof(PortGroupAttribute), true);
                var attributes2 = other.GetType().GetCustomAttributes(typeof(PortGroupAttribute), true);
                if (attributes1.Length > 0 || attributes2.Length > 0)
                {
                    HashSet<int> temp = new HashSet<int>();
                    foreach (PortGroupAttribute item in attributes1)
                    {
                        temp.Add(item.Group);
                    }
                    foreach (PortGroupAttribute item in attributes2)
                    {
                        if (temp.Contains(item.Group))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
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
                if (edges[i] == edgeId)
                {
                    edges.RemoveAt(i);
                }
            }
        }

        /// <summary> Returns TRUE if this port contains a edge with the given edge id </summary>
        /// <param name="edgeId"> The edge id to search for </param>
        public bool ContainsEdge(string edgeId)
        {
            return edges.Any(edge => edge == edgeId);
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
        public void DisconnectFromNode(string nodeId, GraphBase graphBase)
        {
            if (!isConnected) return;
            for (int i = edges.Count - 1; i >= 0; i--) 
            {
                var edge = graphBase.GetEdge(edges[i]);
                if (isInput && edge.outputNodeId == nodeId) edges.RemoveAt(i); 
                if (isOutput && edge.inputNodeId == nodeId) edges.RemoveAt(i);
            }
        }
        
        #endregion

        #region private Methods

        /// <summary> Returns TRUE if this port is connected to the given port id </summary>
        /// <param name="portId"> The port id to search for and determine if this port is connected to or not </param>
        private bool IsConnectedToPort(string portId, GraphBase graphBase)
        {
            foreach (var edgeId in edges) //iterate through all the connections list
            {
                var edge = graphBase.GetEdge(edgeId);
                if (isInput && edge.outputPortId == portId)return true; //if this is an input port -> look for the port id at the output port of the connection
                if (isOutput && edge.inputPortId == portId)return true; //if this is an output port -> look for the port id at the input port of the connection
            }
            return false;
        }
        
        /// <summary> Generates a new unique port id for this port and returns the newly generated id value </summary>
        private string GenerateNewId()
        {
            id = Guid.NewGuid().ToString();
            return id;
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