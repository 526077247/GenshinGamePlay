using System.Collections.Generic;
using System.IO;
using DaGenGraph;
using DaGenGraph.Editor;
using Unity.Code.NinoGen;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupGraphWindow : GraphWindow<SceneGroupGraph>
    {
        public string path;

        internal static SceneGroupGraphWindow instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;
                var windows = Resources.FindObjectsOfTypeAll<SceneGroupGraphWindow>();
                s_Instance = windows.Length > 0 ? windows[0] : null;
                if (s_Instance != null) return s_Instance;
                s_Instance = CreateWindow<SceneGroupGraphWindow>();
                return s_Instance;
            }
        }

        private static SceneGroupGraphWindow s_Instance;

        [MenuItem("Tools/Graph编辑器/关卡编辑器")]
        public static void GetWindow()
        {
            instance.titleContent = new GUIContent("关卡编辑器");
            instance.Show();
            instance.InitGraph();
        }

        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeData(data, path);
        }

        private static bool InitializeData(TextAsset asset, string path)
        {
            if (asset == null) return false;
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<SceneGroupGraph>(asset.text, out var aiJson))
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
            AddButton(new GUIContent("导出"), Export);
        }

        protected override void InitGraph()
        {
            path = null;
            base.InitGraph();
        }

        protected override SceneGroupGraph CreateGraph()
        {
            var res = new SceneGroupGraph();
            return res;
        }

        protected override SceneGroupGraph LoadGraph()
        {
            string searchPath = EditorUtility.OpenFilePanel($"新建{nameof(SceneGroupGraph)}配置文件",
                "Assets/AssetsPackage/GraphAssets/SceneGroup", "json");
            if (!string.IsNullOrEmpty(searchPath))
            {
                var jStr = File.ReadAllText(searchPath);
                var obj = JsonHelper.FromJson<SceneGroupGraph>(jStr);
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
                string searchPath = EditorUtility.SaveFilePanel($"新建{nameof(SceneGroupGraph)}配置文件",
                    "Assets/AssetsPackage/GraphAssets/SceneGroup", nameof(SceneGroupGraph), "json");
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
            
            menu.AddItem(new GUIContent("Create/新建阶段"), false,
                () => { CreateNodeView(m_Graph.CreateNode<SceneGroupSuitesNode>(current.mousePosition, "阶段")); });
            if (!string.IsNullOrEmpty(m_Graph.startNodeId))
            {
                menu.AddItem(new GUIContent("Create/路径/新建路径"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<RouteNode>(current.mousePosition, "路径")); });
                menu.AddItem(new GUIContent("Create/路径/路径点"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<WaypointNode>(current.mousePosition, "路径点")); });
                menu.AddItem(new GUIContent("Create/事件/新建监听"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerNode>(current.mousePosition, "监听事件")); });
                menu.AddItem(new GUIContent("Create/事件/执行项"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerActionNode>(current.mousePosition, "执行项")); });
                menu.AddItem(new GUIContent("Create/事件/判断项"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerConditionNode>(current.mousePosition, "判断项")); });
                menu.AddItem(new GUIContent("Create/事件/重设寻路路径"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupRestartPlatformMoveNode>(current.mousePosition, "重设寻路路径")); });
            }
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
                if (port is WaypointPort)
                {
                    menu.AddItem(new GUIContent("下一路径点"), false, () =>
                    {
                        var node = m_Graph.CreateNode<WaypointNode>(current.mousePosition, "路径点");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
                else if (port is SetRouteIdPort)
                {
                    menu.AddItem(new GUIContent("寻路路径"), false, () =>
                    {
                        var node = m_Graph.CreateNode<RouteNode>(current.mousePosition, "路径");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
                else if (port is SceneGroupTriggerPort)
                {
                    menu.AddItem(new GUIContent("监听"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupTriggerNode>(current.mousePosition, "监听事件");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
                else if (port is SceneGroupActionPort)
                {
                    menu.AddItem(new GUIContent("执行"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupTriggerActionNode>(current.mousePosition, "执行项");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent("判断"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupTriggerConditionNode>(current.mousePosition, "判断项");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent("重设寻路路径"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupRestartPlatformMoveNode>(current.mousePosition, "重设寻路路径");
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
                menu.AddItem(new GUIContent("Delete"), false, () => { DeleteNode(nodeBase); });
            }
        }

        #endregion

        #region Export

        private void Export()
        {
            if (!string.IsNullOrEmpty(path))
            {
                var name = Path.GetFileNameWithoutExtension(this.path);
                var path = EditorUtility.SaveFilePanel($"新建SceneGraphGraph配置文件",
                    "Assets/AssetsPackage/EditConfig/SceneGraph/", name, "bytes");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var obj = Convert(m_Graph);
                File.WriteAllText(path.Replace("bytes","json"),JsonHelper.ToJson(obj));
                File.WriteAllBytes(path, obj.Serialize());
                AssetDatabase.Refresh();
                Debug.Log("导出成功");
            }
            else
            {
                Debug.LogError("请先保存");
            }

        }

        private ConfigSceneGroup Convert(SceneGroupGraph graph)
        {
            ConfigSceneGroup res = new ConfigSceneGroup();
            res.Id = graph.Id;
            res.Remarks = graph.Remarks;
            res.Actors = graph.Actors;
            // res.Clips = new ConfigStorySerialClip();
            // if (graph.GetStartNode() != null)
            // {
            //     List<ConfigStoryClip> clips = new List<ConfigStoryClip>();
            //     Convert(graph.GetStartNode(), clips);
            //     res.Clips.Clips = clips.ToArray();
            // }
            return res;
        }
        //
        // private void Convert(NodeBase clipNode, List<ConfigStoryClip> clips)
        // {
        //     if (clipNode == null) return;
        //     if (clipNode is StoryClipNode storyClipNode)
        //     {
        //         if (storyClipNode.Data != null)
        //             clips.Add(storyClipNode.Data);
        //     }
        //     else if (clipNode is StoryBranchClipNode storyBranchClipNode)
        //     {
        //         ConfigStoryBranchClip branchClip = new();
        //         List<ConfigStoryBranchClipItem> choices = new List<ConfigStoryBranchClipItem>();
        //         foreach (var item in storyBranchClipNode.outputPorts)
        //         {
        //             if (item is StoryBranchPort)
        //             {
        //                 if (item.edges != null && item.edges.Count > 0)
        //                 {
        //                     var edge = m_Graph.GetEdge(item.edges[0]);
        //                     var node = m_Graph.FindNode(edge.inputNodeId) as StoryBranchClipItemNode;
        //                     ConfigStoryBranchClipItem clipItem = Convert(node);
        //                     choices.Add(clipItem);
        //                 }
        //             }
        //         }
        //         branchClip.Branchs = choices.ToArray();
        //         clips.Add(branchClip);
        //     }
        //     else if (clipNode is StoryParallelClipNode storyParallelClipNode)
        //     {
        //         ConfigStoryParallelClip parallelClip = new();
        //         List<ConfigStoryClip> parallels = new List<ConfigStoryClip>();
        //         foreach (var item in storyParallelClipNode.outputPorts)
        //         {
        //             if (item.portName!="下一步")
        //             {
        //                 if (item.edges != null && item.edges.Count > 0)
        //                 {
        //                     var edge = m_Graph.GetEdge(item.edges[0]);
        //                     var node = m_Graph.FindNode(edge.inputNodeId);
        //                     List<ConfigStoryClip> newClips = new List<ConfigStoryClip>();
        //                     Convert(node, newClips);
        //                     var clip = Convert(newClips);
        //                     if (clip != null)
        //                     {
        //                         parallels.Add(clip);
        //                     }
        //                 }
        //             }
        //         }
        //
        //         parallelClip.WaitAll = storyParallelClipNode.WaitAll;
        //         parallelClip.Clips = parallels.ToArray();
        //     }
        //     var next = clipNode.GetFirstOutputPort();
        //     if (next.edges != null && next.edges.Count > 0)
        //     {
        //         var edge = m_Graph.GetEdge(next.edges[0]);
        //         var node = m_Graph.FindNode(edge.inputNodeId);
        //         Convert(node, clips);
        //     }
        // }
        //
        // private ConfigStoryBranchClipItem Convert(StoryBranchClipItemNode batchItem)
        // {
        //     ConfigStoryBranchClipItem res = new ConfigStoryBranchClipItem();
        //     var item = batchItem.GetFirstOutputPort();
        //     if (item.edges != null && item.edges.Count > 0)
        //     {
        //         var edge = m_Graph.GetEdge(item.edges[0]);
        //         var node = m_Graph.FindNode(edge.inputNodeId);
        //         List<ConfigStoryClip> newClips = new List<ConfigStoryClip>();
        //         Convert(node, newClips);
        //         res.Clip = Convert(newClips);
        //     }
        //
        //     res.Text = batchItem.Text;
        //     return res;
        // }
        //
        // private ConfigStoryClip Convert(List<ConfigStoryClip> clips)
        // {
        //     if (clips.Count > 1)
        //     {
        //         ConfigStorySerialClip serialClip = new ConfigStorySerialClip();
        //         serialClip.Clips = clips.ToArray();
        //         clips.Add(serialClip);
        //         return serialClip;
        //     }
        //     else if (clips.Count == 1)
        //     {
        //         return clips[0];
        //     }
        //
        //     return null;
        // }

        #endregion

    }
}