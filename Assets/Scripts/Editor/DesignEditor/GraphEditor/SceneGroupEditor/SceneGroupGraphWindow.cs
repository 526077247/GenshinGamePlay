using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class SceneGroupGraphWindow : GraphWindow<SceneGroupGraph>
    {
        public string path;

        internal static SceneGroupGraphWindow Instance
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
            Instance.titleContent = new GUIContent("关卡编辑器");
            Instance.Show();
            Instance.InitGraph();
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
                var win = Instance;
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

        protected override Type FallBackNodeViewType()
        {
            return typeof(SceneGroupNodeView);
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
                menu.AddItem(new GUIContent("Create/监听/事件"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerNode>(current.mousePosition, "监听事件")); });
                menu.AddItem(new GUIContent("Create/事件/执行项"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerActionNode>(current.mousePosition, "执行项")); });
                menu.AddItem(new GUIContent("Create/事件/判断项"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerConditionNode>(current.mousePosition, "判断项")); });
                menu.AddItem(new GUIContent("Create/事件/逻辑判断项"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerLogicConditionNode>(current.mousePosition, "逻辑判断项")); });
                menu.AddItem(new GUIContent("Create/事件/重设寻路路径"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupRestartPlatformMoveNode>(current.mousePosition, "重设寻路路径")); });
                menu.AddItem(new GUIContent("Create/事件/等待执行"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<SceneGroupTriggerWaitNode>(current.mousePosition, "等待执行")); });
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
                    menu.AddItem(new GUIContent("逻辑判断"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupTriggerLogicConditionNode>(current.mousePosition, "逻辑判断项");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent("重设寻路路径"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupRestartPlatformMoveNode>(current.mousePosition, "重设寻路路径");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent("等待执行"), false, () =>
                    {
                        var node = m_Graph.CreateNode<SceneGroupTriggerWaitNode>(current.mousePosition, "等待执行");
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

        protected override void RefreshValueDropDown(FieldInfo field, object obj, string valuesGetter)
        {
            if (ValueDropDownHelper.RefreshValueDropDown(m_Graph, field, obj, valuesGetter, valueDropdown))
            {
                return;
            }

            base.RefreshValueDropDown(field, obj, valuesGetter);
        }

        #endregion

        #region Export

        private void Export()
        {
            if (!string.IsNullOrEmpty(path))
            {
                var name = Path.GetFileNameWithoutExtension(this.path);
                var path = EditorUtility.SaveFilePanel($"新建SceneGraphGraph配置文件",
                    "Assets/AssetsPackage/EditConfig/SceneGroup/", name, "json");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var obj = Convert(m_Graph);
                File.WriteAllText(path,JsonHelper.ToJson(obj));
#if RoslynAnalyzer
                File.WriteAllBytes(path.Replace("json","bytes"), obj.Serialize());
#endif
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
            res.Position = graph.Position;
            res.Rotation = graph.Rotation;
            res.Actors = graph.Actors;
            res.Zones = graph.Zones;
            res.RandSuite = graph.RandSuite;
            res.InitSuite = graph.InitSuite;

            Dictionary<RouteNode,ConfigRoute> routes = new ();
            Dictionary<SceneGroupTriggerNode, ConfigSceneGroupTrigger> triggers = new();
            List<SceneGroupSuitesNode> temp = new List<SceneGroupSuitesNode>();
            foreach (var node in graph.values)
            {
                if (node is SceneGroupSuitesNode suitesNode)
                {
                    temp.Add(suitesNode);
                }
                else if (node is RouteNode routeNode)
                {
                    if (!routes.TryGetValue(routeNode, out var conf))
                    {
                        conf = Convert(routeNode);
                        if (conf != null)
                        {
                            routes.Add(routeNode, conf);
                        }
                    }
                }
                else if (node is SceneGroupTriggerNode triggerNode)
                {
                    if (!triggers.TryGetValue(triggerNode, out var conf))
                    {
                        conf = Convert(triggerNode,routes);
                        if (conf != null)
                        {
                            conf.LocalId = triggers.Count + 1;
                            triggers.Add(triggerNode, conf);
                        }
                    }
                }
            }
            res.Triggers = triggers.Values.ToArray();
            
            List<ConfigSceneGroupSuites> suitesList = new List<ConfigSceneGroupSuites>();
            foreach (var item in temp)
            {
                var suites = Convert(item,triggers);
                suitesList.Add(suites);
            }
            res.Suites = suitesList.ToArray();
            res.Route = routes.Values.ToArray();
            return res;
        }
        
        private ConfigRoute Convert(RouteNode routeNode)
        {
            if (routeNode == null) return null;
            ConfigRoute res = JsonHelper.FromJson<ConfigRoute>(JsonHelper.ToJson(routeNode.Route));
            if (routeNode.ShowEditorPoints)
            {
                res.Points = routeNode.Points;
            }
            else
            {
                var temp = new List<ConfigWaypoint>();
                foreach (var item in routeNode.outputPorts)
                {
                    if (item is WaypointPort)
                    {
                        if (item.edges != null && item.edges.Count > 0)
                        {
                            var edge = m_Graph.GetEdge(item.edges[0]);
                            var node = m_Graph.FindNode(edge.inputNodeId) as WaypointNode;
                            Convert(node,temp);
                        }
                    }
                }
                res.Points = temp.ToArray();
            }

            return res;
        }

        private void Convert(WaypointNode routeNode, List<ConfigWaypoint> temp)
        {
            if (routeNode == null||routeNode.Point==null) return;
            routeNode.Point.Index = temp.Count;
            temp.Add(routeNode.Point);
            foreach (var item in routeNode.outputPorts)
            {
                if (item is WaypointPort)
                {
                    if (item.edges != null && item.edges.Count > 0)
                    {
                        var edge = m_Graph.GetEdge(item.edges[0]);
                        var node = m_Graph.FindNode(edge.inputNodeId) as WaypointNode;
                        Convert(node,temp);
                    }
                }
            }
        }
        
        private ConfigSceneGroupTrigger Convert(SceneGroupTriggerNode triggerNode,Dictionary<RouteNode,ConfigRoute> routes)
        {
            if (triggerNode == null) return null;
            ConfigSceneGroupTrigger res = JsonHelper.FromJson<ConfigSceneGroupTrigger>(JsonHelper.ToJson(triggerNode.Trigger));
            var temp = new List<ConfigSceneGroupAction>();
            
            foreach (var item in triggerNode.outputPorts)
            {
                if (item is SceneGroupActionPort)
                {
                    if (item.edges != null && item.edges.Count > 0)
                    {
                        for (int i = 0; i < item.edges.Count; i++)
                        {
                            var edge = m_Graph.GetEdge(item.edges[i]);
                            var node = m_Graph.FindNode(edge.inputNodeId);
                            var configNode = ConvertAction(node, routes);
                            temp.Add(configNode);
                        }
                    }
                }
            }
            temp.Sort(CompareConfigSceneGroupAction);
            res.Actions = temp.ToArray();
            return res;
        }

        private ConfigSceneGroupAction ConvertAction(NodeBase nodeBase,Dictionary<RouteNode,ConfigRoute> routes)
        {
            if (nodeBase is SceneGroupTriggerActionNode actionNode)
            {
                return JsonHelper.FromJson<ConfigSceneGroupAction>(JsonHelper.ToJson(actionNode.Action));
            }
            if (nodeBase is SceneGroupRestartPlatformMoveNode moveNode)
            {
                var res = new ConfigSceneGroupRestartPlatformMove();
                res.ActorId = moveNode.ActorId;
                foreach (var item in nodeBase.outputPorts)
                {
                    if (item is SetRouteIdPort)
                    {
                        if (item.edges != null && item.edges.Count > 0)
                        {
                            var edge = m_Graph.GetEdge(item.edges[0]);
                            var routeNode = m_Graph.FindNode(edge.inputNodeId) as RouteNode;
                            if (routeNode == null) continue;
                            if (!routes.TryGetValue(routeNode, out var conf))
                            {
                                conf = Convert(routeNode);
                                if (conf != null)
                                {
                                    routes.Add(routeNode, conf);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            res.RouteId = conf.LocalId;
                            break;
                        }
                    }
                }
                
                return res;
            }
            if (nodeBase is SceneGroupTriggerWaitNode waitNode)
            {
                var res = new ConfigSceneGroupDelayAction();
                res.Delay = waitNode.Delay;
                res.IsRealTime = waitNode.IsRealTime;
                var actions = new List<ConfigSceneGroupAction>();
                foreach (var item in nodeBase.outputPorts)
                {
                    if (item is SceneGroupActionPort)
                    {
                        if (item.edges != null && item.edges.Count > 0)
                        {
                            for (int i = 0; i < item.edges.Count; i++)
                            {
                                var edge = m_Graph.GetEdge(item.edges[i]);
                                var node = m_Graph.FindNode(edge.inputNodeId);
                                var configNode = ConvertAction(node,routes);
                                actions.Add(configNode);
                            }
                           
                        }
                    }
                }

                actions.Sort(CompareConfigSceneGroupAction);
                res.Actions = actions.ToArray();
                return res;
            }
            ConfigSceneGroupConditionAction conditionAction = null;
            if (nodeBase is SceneGroupTriggerConditionNode conditionNode)
            {
                var newNode = new ConfigSceneGroupNormalConditionAction();
                newNode.Conditions = conditionNode.Condition;
                conditionAction = newNode;
            }
            else if (nodeBase is SceneGroupTriggerLogicConditionNode logicNode)
            {
                //逻辑或(True)与(False)
                if (logicNode.Mode)
                {
                    var newNode = new ConfigSceneGroupOrAction();
                    newNode.Conditions = logicNode.Condition;
                    conditionAction = newNode;
                }
                else
                {
                    var newNode = new ConfigSceneGroupAndAction();
                    newNode.Conditions = logicNode.Condition;
                    conditionAction = newNode;
                }

            }
            else
            {
                return null;
            }

            var fail = new List<ConfigSceneGroupAction>();
            var succ = new List<ConfigSceneGroupAction>();
            foreach (var item in nodeBase.outputPorts)
            {
                if (item is SceneGroupActionPort)
                {
                    if (item.edges != null && item.edges.Count > 0)
                    {
                        for (int i = 0; i < item.edges.Count; i++)
                        {
                            var edge = m_Graph.GetEdge(item.edges[i]);
                            var node = m_Graph.FindNode(edge.inputNodeId);
                            var configNode = ConvertAction(node,routes);
                            if (item.portName == "不满足则")
                                fail.Add(configNode);
                            else if (item.portName == "满足条件后")
                                succ.Add(configNode);
                        }
                    }
                }
            }
            fail.Sort(CompareConfigSceneGroupAction);
            succ.Sort(CompareConfigSceneGroupAction);
            conditionAction.Fail = fail.ToArray();
            conditionAction.Success = succ.ToArray();
            return conditionAction;
        }

        private ConfigSceneGroupSuites Convert(SceneGroupSuitesNode suitesNode,
            Dictionary<SceneGroupTriggerNode, ConfigSceneGroupTrigger> triggers)
        {
            if (suitesNode == null) return null;
            ConfigSceneGroupSuites res = new ConfigSceneGroupSuites();
            res.Actors = suitesNode.Actors;
            res.Remarks = suitesNode.Remarks;
            res.LocalId = suitesNode.Id;
            res.Zones = suitesNode.Zones;
            res.RandWeight = suitesNode.RandWeight;
            List<int> temp = new();
            foreach (var item in suitesNode.outputPorts)
            {
                if (item is SceneGroupTriggerPort)
                {
                    if (item.edges != null && item.edges.Count > 0)
                    {
                        for (int i = 0; i < item.edges.Count; i++)
                        {
                            var edge = m_Graph.GetEdge(item.edges[i]);
                            var triggerNode = m_Graph.FindNode(edge.inputNodeId) as SceneGroupTriggerNode;
                            if (triggerNode == null) continue;
                            if (!triggers.TryGetValue(triggerNode, out var conf))
                            {
                                continue;
                            }
                            temp.Add(conf.LocalId);
                        }
                    }
                }
            }

            res.Triggers = temp.ToArray();
            return res;
        }

        private int CompareConfigSceneGroupAction(ConfigSceneGroupAction a, ConfigSceneGroupAction b)
        {
            return a.LocalId - b.LocalId;
        }

        #endregion

    }
}