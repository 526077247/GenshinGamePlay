using System;
using UnityEngine;

namespace DaGenGraph
{
    [Serializable]
    public class VirtualPoint
    {
        /// <summary> The Node this connection point belongs to </summary>
        public Node node;

        /// <summary> The Port this connection point belongs top </summary>
        public Port port;

        /// <summary> The Point position in Node Space (not World Space) </summary>
        public Vector2 pointPosition;

        /// <summary>  The Point position in Port Space (not Node or World Space). This is the value found in the Port.ConnectionPoints list </summary>
        public Vector2 localPointPosition;

        /// <summary>
        ///     The Rect of this connection point in World Space.
        ///     <para />
        ///     Note that CalculateRect() needs to be called before using this rect.
        ///     <para />
        ///     CalculateRect() is done automatically when building the Database or when the Database is updated.
        ///     <para />
        ///     The Database update is executed automatically when (Event.current.type == EventType.MouseDrag) is performed on the parent Node.
        /// </summary>
        public Rect rect;

        /// <summary>
        ///     Remembers if this point is occupied or not.
        ///     <para />
        ///     A point is deemed occupied when in the ConnectionsDatabase there is at least one VirtualConnection that uses this PointPosition (for the parent Port, for the parent Node)
        /// </summary>
        public bool isConnected;


        /// <summary> Construct a Virtual Point </summary>
        /// <param name="node"> The node this virtual point belongs to </param>
        /// <param name="port"></param>
        /// <param name="pointPosition"> The position - in node space - this point is located at </param>
        /// <param name="localPointPosition"> The position - in socket space - this point is located at </param>
        public VirtualPoint(Node node, Port port, Vector2 pointPosition, Vector2 localPointPosition)
        {
            this.node = node;
            this.port = port;
            this.pointPosition = pointPosition;
            this.localPointPosition = localPointPosition;
            isConnected = false;
            CalculateRect(); //calculate the Rect by converting the position from NodeSpace to WorldSpace and setting up correct values
        }

        /// <summary>
        ///     Calculate this virtual point's rect by converting it's point position node space position into a world position.
        ///     <para />
        ///     It uses the default connector width and height (found in Settings.GUI) in order to create the rect.
        /// </summary>
        public void CalculateRect()
        {
            rect = new Rect();
            if (node == null)
            {
                Debug.Log("Cannot calculate the connection point Rect because the parent Node is null.");
                return;
            }

            if (port == null)
            {
                Debug.Log("Cannot calculate the connection point Rect because the parent Port is null.");
                return;
            }

            var worldRect = new Rect(node.GetX(),
                node.GetY() + port.GetY(),
                port.GetWidth(),
                port.GetHeight());

            rect = new Rect(worldRect.x + pointPosition.x + m_ConnectionPointWidth / 2,
                worldRect.y + pointPosition.y + m_ConnectionPointWidth / 2,
                m_ConnectionPointWidth,
                m_ConnectionPointHeight);
        }

        private float m_ConnectionPointWidth = 16f;
        private float m_ConnectionPointHeight = 16f;
    }
}