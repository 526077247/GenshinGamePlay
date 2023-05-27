using System;
using Unity.Collections;
using UnityEngine;

namespace DaGenGraph
{
    [SerializeField]
    public class Edge
    {
        #region Properties

        /// <summary> [Editor Only] Pings this Edge </summary>
        public bool ping { get; set; }

        /// <summary> [Editor Only] ReSet Animation Time </summary>
        public bool reSetTime { get; set;}

        /// <summary> Returns the id of this Edge </summary>
        public string id
        {
            get => m_ID;
            set => m_ID = value;
        }

        /// <summary> Returns the id of this Edge's input Node </summary>
        public string inputNodeId
        {
            get => m_InputNodeId;
            set => m_InputNodeId = value;
        }

        /// <summary> Returns the id of this Edge's input Port </summary>
        public string inputPortId
        {
            get => m_InputPortId;
            set => m_InputPortId = value;
        }

        /// <summary> Returns the id of this Edge's output Node </summary>
        public string outputNodeId
        {
            get => m_OutputNodeId;
            set => m_OutputNodeId = value;
        }

        /// <summary> Returns the id of this Edge's output Port </summary>
        public string outputPortId
        {
            get => m_OutputPortId;
            set => m_OutputPortId = value;
        }

        /// <summary> [Editor Only] Returns the position of this Edge's input point </summary>
        public Vector2 inputEdgePoint
        {
            get => m_InputEdgePoint;
            set => m_InputEdgePoint = value;
        }

        /// <summary> [Editor Only] Returns the position of this Edge's output point </summary>
        public Vector2 outputEdgePoint
        {
            get => m_OutputEdgePoint;
            set => m_OutputEdgePoint = value;
        }

        #endregion

        #region Private Variables

        [SerializeField,ReadOnly] private string m_ID;
        [SerializeField]  private Vector2 m_InputEdgePoint;
        [SerializeField]  private Vector2 m_OutputEdgePoint;
        [SerializeField]  private string m_InputNodeId;
        [SerializeField]  private string m_InputPortId;
        [SerializeField]  private string m_OutputNodeId;
        [SerializeField]  private string m_OutputPortId;

        #endregion

        #region Constructors

        /// <summary> Creates a new instance for this class between two ports (Input - Output or Output - Input) </summary>
        /// <param name="port1"> port One </param>
        /// <param name="port2"> port Two </param>
        public Edge(Port port1, Port port2)
        {
            GenerateNewId();
            if (port1.isOutput && port2.isInput)
            {
                m_InputNodeId = port2.nodeId;
                m_InputPortId = port2.id;
                m_InputEdgePoint = port2.GetClosestEdgePointToPort(port2);

                m_OutputNodeId = port1.nodeId;
                m_OutputPortId = port1.id;
                m_OutputEdgePoint = port1.GetClosestEdgePointToPort(port1);
            }

            if (port2.isOutput && port1.isInput)
            {
                m_InputNodeId = port1.nodeId;
                m_InputPortId = port1.id;
                m_InputEdgePoint = port1.GetClosestEdgePointToPort(port1);

                m_OutputNodeId = port2.nodeId;
                m_OutputPortId = port2.id;
                m_OutputEdgePoint = port2.GetClosestEdgePointToPort(port2);
            }
        }

        #endregion

        #region private Methods

        /// <summary> Generates a new id for this connections and returns it </summary>
        private string GenerateNewId()
        {
            m_ID = Guid.NewGuid().ToString();
            return m_ID;
        }

        #endregion
        
    }
}