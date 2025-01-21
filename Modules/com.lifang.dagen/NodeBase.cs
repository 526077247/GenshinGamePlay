using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph
{
    [Serializable]
    public abstract class NodeBase: ScriptableObject
    {
        #region public Variables

        [DrawIgnore] public List<Port> inputPorts;
        [DrawIgnore] public List<Port> outputPorts;
        [DrawIgnore] public bool allowDuplicateNodeName;
        [DrawIgnore] public bool allowEmptyNodeName;
        [DrawIgnore] public bool canBeDeleted;
        [DrawIgnore] public float height;
        [DrawIgnore] public float width;
        [DrawIgnore] public float x;
        [DrawIgnore] public float y;
        [DrawIgnore] public int minimumInputPortsCount;
        [DrawIgnore] public int minimumOutputPortsCount;
        [DrawIgnore] public string id;
        [DrawIgnore] public bool isHovered;
        [DrawIgnore] public bool errorNodeNameIsEmpty;
        [DrawIgnore] public bool errorDuplicateNameFoundInGraph;

        /// <summary> Trigger a visual cue for this node, in the Editor, at runtime. Mostly used when this node has been activated </summary>
        [DrawIgnore] public bool ping;

        public event DeletePortDelegate onDeletePort;

        public delegate void DeletePortDelegate(Port port);

        #endregion

        #region Editor

        public virtual bool GetHasErrors() => errorNodeNameIsEmpty || errorDuplicateNameFoundInGraph;


        /// <summary> Returns the first input port. If there isn't one, it returns null </summary>
        public Port GetFirstInputPort() => inputPorts.Count > 0 ? inputPorts[0] : null;

        /// <summary> Returns the first output port. If there isn't one, it returns null </summary>
        public Port GetFirstOutputPort() => outputPorts.Count > 0 ? outputPorts[0] : null;
        

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


        /// <summary> Set to allow this node to have an empty node name </summary>
        /// <param name="value"> Disable error for empty node name </param>
        protected void SetAllowEmptyNodeName(bool value)
        {
            allowEmptyNodeName = value;
        }

        /// <summary> Set to allow this node to have a duplicate node name </summary>
        /// <param name="value"> Disable error for duplicate node name </param>
        protected void SetAllowDuplicateNodeName(bool value)
        {
            allowDuplicateNodeName = value;
        }

        #endregion

        #region Public Methods

        /// <summary> Returns the x coordinate of this node </summary>
        public float GetX()
        {
            return x;
        }

        /// <summary> Returns the y coordinate of this node </summary>
        public float GetY()
        {
            return y;
        }

        /// <summary> Returns the width of this node </summary>
        public float GetWidth()
        {
            return width;
        }

        /// <summary> Returns the height of this node </summary>
        public float GetHeight()
        {
            return height;
        }

        /// <summary> Returns the position of this node </summary>
        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }

        /// <summary> Returns the size of this node (x is width, y is height) </summary>
        public Vector2 GetSize()
        {
            return new Vector2(GetWidth(), height);
        }

        /// <summary> Returns the Rect of this node </summary>
        public Rect GetRect()
        {
            return new Rect(x, y, GetWidth(), height);
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
            x = position.x;
            y = position.y;
        }

        /// <summary> Set the position of this node's Rect </summary>
        /// <param name="x"> The new x coordinate value </param>
        /// <param name="y"> The new y coordinate value </param>
        public void SetPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary> Set the Rect values for this node </summary>
        /// <param name="rect"> The new rect values </param>
        public void SetRect(Rect rect)
        {
            x = rect.x;
            y = rect.y;
            width = rect.width;
            height = rect.height;
        }

        /// <summary> Set the Rect values for this node </summary>
        /// <param name="position"> The new position value </param>
        /// <param name="size"> The new size value </param>
        public void SetRect(Vector2 position, Vector2 size)
        {
            x = position.x;
            y = position.y;
            width = size.x;
            height = size.y;
        }

        /// <summary> Set the Rect values for this node </summary>
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

        /// <summary> Set the size of this node's Rect </summary>
        /// <param name="size"> The new node size (x is width, y is height) </param>
        public void SetSize(Vector2 size)
        {
            width = size.x;
            height = size.y;
        }

        /// <summary> Set the size of this node's Rect </summary>
        /// <param name="width"> The new width value </param>
        /// <param name="height"> The new height value </param>
        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary> Set the width of this node's Rect </summary>
        /// <param name="value"> The new width value </param>
        public void SetWidth(float value)
        {
            width = value;
        }

        /// <summary> Set the height of this node's Rect </summary>
        /// <param name="value"> The new height value </param>
        public void SetHeight(float value)
        {
            height = value;
        }

        /// <summary> Set the x coordinate of this node's Rect </summary>
        /// <param name="value"> The new x value </param>
        public void SetX(float value)
        {
            x = value;
        }

        /// <summary> Set the y coordinate of this node's Rect </summary>
        /// <param name="value"> The new y value </param>
        public void SetY(float value)
        {
            y = value;
        }

        /// <summary> Convenience method to add a new input port to this node </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="edgeType"> The port edge points locations (if null or empty, it will automatically add two edge points to the left of and the right of the port) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddInputPort(string portName, EdgeMode edgeMode, bool canBeDeleted, EdgeType edgeType,
            bool canBeReordered = true)
        {
            return AddPort(portName, PortDirection.Input, edgeMode, canBeDeleted, canBeReordered, edgeType);
        }

        /// <summary> Convenience method to add a new input port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddInputPort(string portName, EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort(portName, PortDirection.Input, edgeMode,  canBeDeleted,
                canBeReordered);
        }

        /// <summary> Convenience method to add a new input port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddInputPort(EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort("", PortDirection.Input, edgeMode, canBeDeleted,
                canBeReordered);
        }

        /// <summary> Convenience method to add a new output port to this node </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="edgeType"> The port edge points locations (if null or empty, it will automatically add two edge points to the left of and the right of the port) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddOutputPort(string portName, EdgeMode edgeMode, bool canBeDeleted, EdgeType edgeType ,
            bool canBeReordered)
        {
            return AddPort(portName, PortDirection.Output, edgeMode, canBeDeleted, canBeReordered,edgeType);
        }

        /// <summary> Convenience method to add a new output port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddOutputPort(string portName, EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort(portName, PortDirection.Output, edgeMode, canBeDeleted, canBeReordered);
        }

        /// <summary> Convenience method to add a new output port to this node. This port will have two edge points automatically added to it and they will be to the left of and the right the port </summary>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        public Port AddOutputPort(EdgeMode edgeMode, bool canBeDeleted, bool canBeReordered)
        {
            return AddPort("", PortDirection.Output, edgeMode,  canBeDeleted, canBeReordered);
        }

        /// <summary> Returns TRUE if the target port can be deleted, after checking is it is marked as 'deletable' and that by deleting it the node minimum ports count does not go below the set threshold </summary>
        /// <param name="port">Target port</param>
        public bool CanDeletePort(Port port)
        {
            //if port is market as cannot be deleted -> return false -> do not allow the dev to delete this port
            if (!port.canBeDeleted) return false;
            //if port is input -> make sure the node has a minimum input ports count before allowing deletion
            if (port.IsInput()) return inputPorts.Count > minimumInputPortsCount;
            //if port is output -> make sure the node has a minimum output ports count before allowing deletion
            if (port.IsOutput()) return outputPorts.Count > minimumOutputPortsCount;
            //event though the port can be deleted -> the node needs to hold a minimum number of ports and will not allow to delete this port
            return false;
        }

        /// <summary> Returns TRUE if a edge with the given id can be found on one of this node's ports </summary>
        /// <param name="edgeId"> Target edge id </param>
        public bool ContainsEdge(string edgeId)
        {
            foreach (var port in inputPorts)
            {
                if (port.edges.Contains(edgeId)) return true;
            }

            foreach (var port in outputPorts)
            {
                if (port.edges.Contains(edgeId)) return true;
            }
            return false;
        }

        public void DeletePort(Port port)
        {
            if (port.IsInput()) inputPorts.Remove(port);
            if (port.IsOutput()) outputPorts.Remove(port);
            onDeletePort?.Invoke(port);
            DeletePortBase(port);
        }
        #endregion

        #region Private Methods

        /// <summary> Adds a port to this node </summary>
        /// <param name="portName"> The name of the port (if null or empty, it will be auto-generated) </param>
        /// <param name="direction"> The port direction (Input/Output) </param>
        /// <param name="edgeMode"> The port edge mode (Multiple/Override) </param>
        /// <param name="edgeType"> The port edge points locations (if null or empty, it will automatically add two edge points to the left of and the right of the port) </param>
        /// <param name="canBeDeleted"> Determines if this port is a special port that cannot be deleted </param>
        /// <param name="canBeReordered"> Determines if this port is a special port that cannot be reordered </param>
        private Port AddPort(string portName, PortDirection direction, EdgeMode edgeMode, 
            bool canBeDeleted, bool canBeReordered , EdgeType edgeType = EdgeType.Both)
        {
            var portNames = new List<string>();
            int counter;
            switch (direction)
            {
                case PortDirection.Input:
                    foreach (Port port in inputPorts)
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

                    var inputPort = CreatePortBase();
                    inputPort.Init(this, portName, direction, edgeMode, edgeType, canBeDeleted, canBeReordered);
                    inputPorts.Add(inputPort);
                    return inputPort;
                case PortDirection.Output:
                    foreach (Port port in outputPorts)
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
                    var outputPort = CreatePortBase();
                    outputPort.Init(this, portName, direction, edgeMode, edgeType, canBeDeleted, canBeReordered);
                    outputPorts.Add(outputPort);
                    return outputPort;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary> Generates a new unique node id for this node and returns the newly generated id value </summary>
        private void GenerateNewId()
        {
            id = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public virtual Methods
        
        protected virtual Port CreatePortBase() 
        {
            var node = CreateInstance<Port>();
            node.name = "Port";
            AssetDatabase.AddObjectToAsset(node,this);
            return node;
        }
        
        protected virtual void DeletePortBase(Port port)
        {
            DestroyImmediate(port,true);
        }

        /// <summary> OnEnterNode is called on the frame when this node becomes active just before any of the node's Update methods are called for the first time </summary>
        /// <param name="previousActiveNode"> The node that was active before this one </param>
        /// <param name="edge"> The edge that activated this node </param>
        public virtual void OnEnter(NodeBase previousActiveNode, Edge edge)
        {
            ping = true;
        }

        /// <summary> OnExitNode is called just before this node becomes inactive </summary>
        /// <param name="nextActiveNode"> The node that will become active next</param>
        /// <param name="edge"> The edge that activates the next node </param>
        public virtual void OnExit(NodeBase nextActiveNode, Edge edge)
        {
            ping = false;
            if (edge != null)
            {
                edge.ping = true;
                edge.reSetTime = true;
            }
        }
        
        public virtual void InitNode(Vector2 pos, string nodeName, int minInputPortsCount = 0, int minOutputPortsCount = 0)
        {
            name = nodeName;
            GenerateNewId();
            inputPorts = new List<Port>();
            outputPorts = new List<Port>();
            canBeDeleted = true;
            this.minimumInputPortsCount = minInputPortsCount;
            this.minimumOutputPortsCount = minOutputPortsCount;
            x = pos.x;
            y = pos.y;
            width = 260f;
            height = 200f;
        }

        public abstract void AddDefaultPorts();

        #endregion
    }
}