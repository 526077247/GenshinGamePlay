using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DaGenGraph
{
    [CreateAssetMenu(fileName = "UIGraph", menuName = "DaGenGraph/Graph", order = 0)]
    public class Graph : SerializedScriptableObject
    {
        [HideInInspector] public Vector2 currentPanOffset = Vector2.zero;
        [HideInInspector] public float currentZoom = 1f;
        [HideInInspector] public Rect graphAreaIncludingTab;
        [HideInInspector] public Rect scaledGraphArea;
        [HideInInspector] public int windowID;
        [HideInInspector] public string scene;
        [HideInInspector] public Node startNode;
        public bool leftInRightOut;
        [SerializeField, HideInInspector] private string m_Guid;
        [SerializeField] public Dictionary<string, Node> nodes { private set; get; }
        private Node activeNode { get; set; }
        private Node previousActiveNode { get; set; }
        public string guid
        {
            get
            {
                if (string.IsNullOrEmpty(m_Guid))
                {
                    m_Guid = Guid.NewGuid().ToString();
                }

                return m_Guid;
            }
        }

        public Graph()
        {
            nodes = new Dictionary<string, Node>();
        }

        private void OnEnable()
        {
            scene = string.IsNullOrEmpty(scene) ? SceneManager.GetActiveScene().name : scene;
        }

        /// <summary>
        /// Find Target Node
        /// </summary>
        /// <param name="id">Target Id</param>
        /// <returns></returns>
        public Node FindNode(string id)
        {
            return nodes.Values.FirstOrDefault(t => t.id == id);
        }

        /// <summary> Activate a Node </summary>
        /// <param name="nextActiveNode"> Target Node </param>
        /// <param name="edge"> edge to ping (used as a visual cue in the editor) </param>
        public void SetActiveNode(Node nextActiveNode, Edge edge = null)
        {
            if (activeNode != null)
            {
                activeNode.OnExit(nextActiveNode, edge);
                activeNode.SetActiveGraph(null);
            }

            previousActiveNode = activeNode;


            activeNode = nextActiveNode;
            if (activeNode == null) return;

            activeNode.SetActiveGraph(this);
            activeNode.OnEnter(previousActiveNode, edge);
        }
    }
}