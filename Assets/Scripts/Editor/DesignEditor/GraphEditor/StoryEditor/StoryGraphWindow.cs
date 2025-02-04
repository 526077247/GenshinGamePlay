using System.Collections.Generic;
using System.IO;
using DaGenGraph;
using DaGenGraph.Editor;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class StoryGraphWindow : GraphWindow<StoryGraph>
    {
        public string path;
        
        internal static StoryGraphWindow instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;
                var windows = Resources.FindObjectsOfTypeAll<StoryGraphWindow>();
                s_Instance = windows.Length > 0 ? windows[0] : null;
                if (s_Instance != null) return s_Instance;
                s_Instance = CreateWindow<StoryGraphWindow>();
                return s_Instance;
            }
        }

        private static StoryGraphWindow s_Instance;

        [MenuItem("Tools/Graph编辑器/剧情编辑器")]
        public static void GetWindow()
        {
            instance.titleContent = new GUIContent("剧情编辑器");
            instance.Show();
            instance.InitGraph();
        }
        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeData(data,path);
        }
        private static bool InitializeData(TextAsset asset,string path)
        {
            if (asset == null) return false;
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<StoryGraph>(asset.text,out var aiJson))
            {
                var win = instance;
                win.Show();
                win.SetGraph(aiJson);
                win.path = path;
                return true;
            }
            return false;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            AddButton(new GUIContent("导出"),Export);
        }
        
        protected override void InitGraph()
        {
            path = null;
            base.InitGraph();
        }

        protected override StoryGraph CreateGraph()
        {
            var res = new StoryGraph();
            return res;
        }

        protected override StoryGraph LoadGraph()
        {
            string searchPath = EditorUtility.OpenFilePanel($"新建{nameof(StoryGraph)}配置文件",
                "Assets/AssetsPackage/GraphAssets/Story", "json");
            if (!string.IsNullOrEmpty(searchPath))
            { 
                var jStr = File.ReadAllText(searchPath);
                var obj = JsonHelper.FromJson<StoryGraph>(jStr);
                if (obj == null) return null;
                path = searchPath;
                return obj;
            }
            return null;
        }

        protected override void SaveGraph()
        {
            if (string.IsNullOrEmpty(path))
            {
                string searchPath = EditorUtility.SaveFilePanel($"新建{nameof(StoryGraph)}配置文件",
                    "Assets/AssetsPackage/GraphAssets/Story", nameof(StoryGraph), "json");
                if (!string.IsNullOrEmpty(searchPath))
                {
                    path = searchPath;
                }
            }

            File.WriteAllText(path, JsonHelper.ToJson(m_Graph));
            AssetDatabase.Refresh();
        }

        #region Menu
        
        protected override void AddGraphMenuItems(GenericMenu menu)
        {
            var current = Event.current;
            if (m_Graph == null) InitGraph();
            menu.AddItem(new GUIContent("Create/普通节点"), false,
                () => { CreateNodeView(m_Graph.CreateNode<StoryClipNode>(current.mousePosition,"节点")); });
            menu.AddItem(new GUIContent("Create/选择节点"), false,
                () => { CreateNodeView(m_Graph.CreateNode<StoryBranchClipNode>(current.mousePosition,"选择节点")); });
            menu.AddItem(new GUIContent("Create/并行节点"), false,
                () => { CreateNodeView(m_Graph.CreateNode<StoryParallelClipNode>(current.mousePosition,"并行节点")); });
            menu.AddItem(new GUIContent("Create/选项"), false,
                () => { CreateNodeView(m_Graph.CreateNode<StoryBranchClipItemNode>(current.mousePosition,"选项")); });
        }

        protected override void AddPortMenuItems(GenericMenu menu, Port port, bool isLine = false)
        {
            var current = Event.current;
            base.AddPortMenuItems(menu, port, isLine);
            if (m_Graph == null) return;
            if (!isLine && port.canBeDeleted)
            {
                menu.AddItem(new GUIContent("Delete"), false, () => { RemovePort(port); });
            }
            if (port.IsOutput())
            {
                var nodeOutput = m_Graph.FindNode(port.nodeId);
                if (nodeOutput == null) return;
                if (port is StoryBranchPort)
                {
                    menu.AddItem(new GUIContent($"{port.portName}Connect/选项"), false, () =>
                    {
                        var node = m_Graph.CreateNode<StoryBranchClipItemNode>(current.mousePosition, "选项");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent($"{port.portName}Connect/普通节点"), false, () =>
                    {
                        var node = m_Graph.CreateNode<StoryClipNode>(current.mousePosition, "节点");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent($"{port.portName}Connect/选择节点"), false, () =>
                    {
                        var node = m_Graph.CreateNode<StoryBranchClipNode>(current.mousePosition, "选择节点");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent($"{port.portName}Connect/并行节点"), false, () =>
                    {
                        var node = m_Graph.CreateNode<StoryParallelClipNode>(current.mousePosition, "并行节点");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
            }
        }

        protected override void AddNodeMenuItems(GenericMenu menu, NodeBase nodeBase)
        {
            if (nodeBase.canBeDeleted)
            {
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    DeleteNode(nodeBase);
                });
            }

            if (nodeBase is StoryClipNode clipNode)
            {
                if (clipNode.outputPorts.Count <= 0)
                {
                    menu.AddItem(new GUIContent("添加下一步"), false,
                        () => { nodeBase.AddOutputPort("下一步", EdgeMode.Override, true, true); });
                }
            }
            else if (nodeBase is StoryBranchClipNode)
            {
                menu.AddItem(new GUIContent("添加选项"), false,
                    () => { nodeBase.AddOutputPort<StoryBranchPort>("选项", EdgeMode.Override, true,EdgeType.Both, true); });
            }
            else if (nodeBase is StoryParallelClipNode)
            {
                menu.AddItem(new GUIContent("添加并行项"), false,
                    () => { nodeBase.AddOutputPort("并行项", EdgeMode.Override, true, true); });
            }
        }

        #endregion

        #region Export

        private void Export()
        {
            if (!string.IsNullOrEmpty(path))
            {
                var name = Path.GetFileNameWithoutExtension(this.path);
                var path = EditorUtility.SaveFilePanel($"新建StoryGraph配置文件", "Assets/AssetsPackage/EditConfig/Story/", name, "json");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                var obj = Convert(m_Graph);
                File.WriteAllText(path,JsonHelper.ToJson(obj));
#if RoslynAnalyzer
                File.WriteAllBytes(path.Replace("json","bytes"),Serializer.Serialize(obj));
#endif
                AssetDatabase.Refresh();
                Debug.Log("导出成功");   
            }
            else
            {
                Debug.LogError("请先保存");   
            }
        
        }
        
        private ConfigStory Convert(StoryGraph graph)
        {
            ConfigStory res = new ConfigStory();
            res.Id = graph.Id;
            res.Remarks = graph.Remarks;
            res.Actors = graph.Actors;
            res.Clips = new ConfigStorySerialClip();
            if (graph.GetStartNode() != null)
            {
                List<ConfigStoryClip> clips = new List<ConfigStoryClip>();
                Convert(graph.GetStartNode(), clips);
                res.Clips.Clips = clips.ToArray();
            }
            return res;
        }
        
        private void Convert(NodeBase clipNode, List<ConfigStoryClip> clips)
        {
            if (clipNode == null) return;
            if (clipNode is StoryClipNode storyClipNode)
            {
                if (storyClipNode.Data != null)
                    clips.Add(storyClipNode.Data);
            }
            else if (clipNode is StoryBranchClipNode storyBranchClipNode)
            {
                ConfigStoryBranchClip branchClip = new();
                List<ConfigStoryBranchClipItem> choices = new List<ConfigStoryBranchClipItem>();
                foreach (var item in storyBranchClipNode.outputPorts)
                {
                    if (item is StoryBranchPort)
                    {
                        if (item.edges != null && item.edges.Count > 0)
                        {
                            var edge = m_Graph.GetEdge(item.edges[0]);
                            var node = m_Graph.FindNode(edge.inputNodeId) as StoryBranchClipItemNode;
                            ConfigStoryBranchClipItem clipItem = Convert(node);
                            choices.Add(clipItem);
                        }
                    }
                }
                branchClip.Branchs = choices.ToArray();
                clips.Add(branchClip);
            }
            else if (clipNode is StoryParallelClipNode storyParallelClipNode)
            {
                ConfigStoryParallelClip parallelClip = new();
                List<ConfigStoryClip> parallels = new List<ConfigStoryClip>();
                foreach (var item in storyParallelClipNode.outputPorts)
                {
                    if (item.portName!="下一步")
                    {
                        if (item.edges != null && item.edges.Count > 0)
                        {
                            for (int i = 0; i < item.edges.Count; i++)
                            {
                                var edge = m_Graph.GetEdge(item.edges[i]);
                                var node = m_Graph.FindNode(edge.inputNodeId);
                                List<ConfigStoryClip> newClips = new List<ConfigStoryClip>();
                                Convert(node, newClips);
                                var clip = Convert(newClips);
                                if (clip != null)
                                {
                                    parallels.Add(clip);
                                }
                            }
                        }
                    }
                }

                parallelClip.WaitAll = storyParallelClipNode.WaitAll;
                parallelClip.Clips = parallels.ToArray();
            }
            var next = clipNode.GetFirstOutputPort();
            if (next.edges != null && next.edges.Count > 0)
            {
                var edge = m_Graph.GetEdge(next.edges[0]);
                var node = m_Graph.FindNode(edge.inputNodeId);
                Convert(node, clips);
            }
        }

        private ConfigStoryBranchClipItem Convert(StoryBranchClipItemNode batchItem)
        {
            ConfigStoryBranchClipItem res = new ConfigStoryBranchClipItem();
            var item = batchItem.GetFirstOutputPort();
            if (item.edges != null && item.edges.Count > 0)
            {
                var edge = m_Graph.GetEdge(item.edges[0]);
                var node = m_Graph.FindNode(edge.inputNodeId);
                List<ConfigStoryClip> newClips = new List<ConfigStoryClip>();
                Convert(node, newClips);
                res.Clip = Convert(newClips);
            }

            res.Text = batchItem.Text;
            return res;
        }

        private ConfigStoryClip Convert(List<ConfigStoryClip> clips)
        {
            if (clips.Count > 1)
            {
                ConfigStorySerialClip serialClip = new ConfigStorySerialClip();
                serialClip.Clips = clips.ToArray();
                clips.Add(serialClip);
                return serialClip;
            }
            else if (clips.Count == 1)
            {
                return clips[0];
            }

            return null;
        }
        #endregion

    }
}