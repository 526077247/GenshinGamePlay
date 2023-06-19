using System.IO;
using DaGenGraph;
using DaGenGraph.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class AIGraphWindow : GraphWindow
    {
        public string path;
        public static AIGraphWindow initance;
        
        [MenuItem("Tools/Graph编辑器/AiGraph")]
        public static void ShowAIGraph()
        {
            if (initance==null)
            {
                initance = CreateWindow<AIGraphWindow>();
                initance.titleContent = new GUIContent("AI");
            }
            initance.Show();
        }
        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as Graph;
            if (data != null)
            {
                ShowAIGraph();
                initance.Init(data);
                initance.path = AssetDatabase.GetAssetPath(data);
                return true;
            }

            return false;
        }

        protected override void ShowGraphContextMenu()
        {
            var current = Event.current;
            var menu = new GenericMenu();
            if (m_Graph.startNode == null)
            {
                menu.AddItem(new GUIContent("New/AiRootNode"), false,
                    () => { AddNode(CreateRootNode(current.mousePosition)); });
            }
            else
            {
                menu.AddItem(new GUIContent("New/AiActionNode"), false, () => { AddNode(CreateActionNode(current.mousePosition)); });
                menu.AddItem(new GUIContent("New/AiConditionNode"), false, () => { AddNode(CreateConditionNode(current.mousePosition)); });
            }
            menu.ShowAsContext();
        }
        private Node CreateRootNode(Vector2 pos, string name = "Root")
        {
            var node = CreateInstance<AIRootNode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            m_Graph.startNode = node;
            return node;
        }
        private Node CreateActionNode(Vector2 pos, string name = "Action")
        {
            var node = CreateInstance<AIActionNode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            return node;
        }

        private Node CreateConditionNode(Vector2 pos, string name = "Condition")
        {
            var node = CreateInstance<AIConditionNode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            return node;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            AddButton(new GUIContent("保存"),Save);
            AddButton(new GUIContent("导出"),Export);
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel($"新建AIGraph配置文件", "Assets/AssetsPackage/GraphAssets/AITree/", "AIGraph", "asset");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var index = path.IndexOf("Assets/");
                path = path.Substring(index, path.Length - index);
                AssetDatabase.CreateAsset(m_Graph, path);
            }
            else
            {
                EditorUtility.SetDirty(m_Graph);
                AssetDatabase.SaveAssetIfDirty(m_Graph);
            }
        }
        private void Export()
        {
            if (!string.IsNullOrEmpty(path))
            {
                var name = Path.GetFileNameWithoutExtension(this.path);
                var path = EditorUtility.SaveFilePanel($"新建AIGraph配置文件", "Assets/AssetsPackage/EditConfig/AITree/", name, "json");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var jstr = JsonHelper.ToJson(Convert(m_Graph));
                File.WriteAllText(path,jstr);
                AssetDatabase.Refresh();
                Log.Error("导出成功");   
            }
            else
            {
                Log.Error("请先保存");   
            }

        }

        private ConfigAIDecisionTree Convert(Graph graph)
        {
            ConfigAIDecisionTree res = null;
            if (graph.startNode is AIRootNode node)
            {
                res = new ConfigAIDecisionTree();
                res.Type = node.Type;
                for (int i = 0; i < node.outputPorts.Count; i++)
                {
                    if (node.outputPorts[i].portName == "Root")
                    {
                        if (node.outputPorts[i].edges != null && node.outputPorts[i].edges.Count > 0)
                        {
                            res.Node = Convert(m_Graph.FindNode(node.outputPorts[i].edges[0].inputNodeId));
                        }
                    }
                    else if (node.outputPorts[i].portName == "CombatRoot")
                    {
                        if (node.outputPorts[i].edges != null && node.outputPorts[i].edges.Count > 0)
                        {
                            res.CombatNode = Convert(m_Graph.FindNode(node.outputPorts[i].edges[0].inputNodeId));
                        }
                    }
                }
              
            }

            return res;
        }
        
        
        private DecisionNode Convert(Node aiNode)
        {
            if (aiNode is AIConditionNode cnode)
            {
                var res = new DecisionConditionNode();
                res.Condition = cnode.Condition;
                for (int i = 0; i < cnode.outputPorts.Count; i++)
                {
                    if (cnode.outputPorts[i].portName == "True")
                    {
                        if(cnode.outputPorts[i].edges!=null && cnode.outputPorts[i].edges.Count>0)
                        {
                            res.True = Convert(m_Graph.FindNode(cnode.outputPorts[i].edges[0].inputNodeId));
                        }
                        
                    }
                    else if (cnode.outputPorts[i].portName == "False")
                    {
                        if (cnode.outputPorts[i].edges != null && cnode.outputPorts[i].edges.Count > 0)
                        {
                            res.False = Convert(m_Graph.FindNode(cnode.outputPorts[i].edges[0].inputNodeId));
                        }
                    }
                }
                return res;
            }
            if (aiNode is AIActionNode anode)
            {
                var res = new DecisionActionNode();
                res.Act = anode.Data.Act;
                res.Tactic = anode.Data.Tactic;
                res.Move = anode.Data.Move;
                return res;
            }
            return null;
        }
    }
}