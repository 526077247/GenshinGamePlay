using System;
using Unity.Collections;
using UnityEngine;

namespace DaGenGraph
{
    public class Edge: ScriptableObject
    {
        #region Properties

        /// <summary> [Editor Only] Pings this Edge </summary>
        public bool ping { get; set; }

        /// <summary> [Editor Only] ReSet Animation Time </summary>
        public bool reSetTime { get; set;}
        
        #endregion

        #region public Variables

        public string id;
        public Vector2 inputEdgePoint;
        public Vector2 outputEdgePoint;
        public string inputNodeId;
        public string inputPortId;
        public string outputNodeId;
        public string outputPortId;

        #endregion

        #region Constructors
        

        /// <summary> Creates a new instance for this class between two ports (Input - Output or Output - Input) </summary>
        /// <param name="port1"> port One </param>
        /// <param name="port2"> port Two </param>
        public void Init(Port port1, Port port2)
        {
            GenerateNewId();
            if (port1.isOutput && port2.isInput)
            {
                inputNodeId = port2.nodeId;
                inputPortId = port2.id;
                inputEdgePoint = port2.GetClosestEdgePointToPort(port2);

                outputNodeId = port1.nodeId;
                outputPortId = port1.id;
                outputEdgePoint = port1.GetClosestEdgePointToPort(port1);
            }

            if (port2.isOutput && port1.isInput)
            {
                inputNodeId = port1.nodeId;
                inputPortId = port1.id;
                inputEdgePoint = port1.GetClosestEdgePointToPort(port1);

                outputNodeId = port2.nodeId;
                outputPortId = port2.id;
                outputEdgePoint = port2.GetClosestEdgePointToPort(port2);
            }
        }

        #endregion

        #region private Methods

        /// <summary> Generates a new id for this connections and returns it </summary>
        private string GenerateNewId()
        {
            id = Guid.NewGuid().ToString();
            return id;
        }

        #endregion
        
    }
}