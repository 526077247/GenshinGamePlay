using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class UIScriptController
    {
        static string addressable_path = "Assets/AssetsPackage/";
        static string generate_path = "Game";
        static bool forced_coverage = false; //是否强制覆盖

        public static bool AllowGenerate(GameObject go, string path)
        {
            if (!go.name.StartsWith("UI"))
            {
                UnityEngine.Debug.LogError(go.name + "没有以UI开头");
                return false;
            }

            if (!go.name.EndsWith("View") && !go.name.EndsWith("Win") && !go.name.EndsWith("Panel") &&
                !go.name.EndsWith("Item"))
            {
                UnityEngine.Debug.LogError(go.name + "没有以View、Win、Panel或者Item结尾");
                return false;
            }

            return path.Contains(addressable_path);
        }

        /// <summary>
        /// 根据选中的节点生成UI代码
        /// </summary>
        public static void GenerateUICode(GameObject[] gos, string path)
        {
            if (gos == null || gos.Length == 0)
            {
                UnityEngine.Debug.LogError("未选中节点");
                return;
            }

            GenerateUIBaseViewCode(gos, path);
        }

        static Dictionary<Type, string> WidgetInterfaceList;

        static UIScriptController() //优先生成的排前面
        {
            WidgetInterfaceList = new Dictionary<Type, string>();
            WidgetInterfaceList.Add(typeof(SuperScrollView.LoopListView2), "UILoopListView2");
            WidgetInterfaceList.Add(typeof(SuperScrollView.LoopGridView), "UILoopGridView");
            WidgetInterfaceList.Add(typeof(CopyGameObject), "UICopyGameObject");
            WidgetInterfaceList.Add(typeof(PointerClick), "UIPointerClick");
            WidgetInterfaceList.Add(typeof(Button), "UIButton");
            WidgetInterfaceList.Add(typeof(InputField), "UIInput");
            WidgetInterfaceList.Add(typeof(Slider), "UISlider");
            WidgetInterfaceList.Add(typeof(Dropdown), "UIDropdown");
            WidgetInterfaceList.Add(typeof(Toggle), "UIToggle");
            WidgetInterfaceList.Add(typeof(Image), "UIImage");
            WidgetInterfaceList.Add(typeof(RawImage), "UIRawImage");
            WidgetInterfaceList.Add(typeof(Text), "UIText");
            WidgetInterfaceList.Add(typeof(TMPro.TMP_Text), "UITextmesh");
            WidgetInterfaceList.Add(typeof(TMPro.TMP_InputField), "UIInputTextmesh");
        }

        static void GenerateUIBaseViewCode(GameObject[] gos, string path)
        {
            if (gos == null || gos.Length == 0) return;

            Transform rootTrans = gos[0].transform;
            while (rootTrans.parent != null)
            {
                rootTrans = rootTrans.parent;
            }

            // 根节点名可能不是prefab名（如 "Canvas (Environment)"），类名统一取prefab文件名
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name))
            {
                name = rootTrans.gameObject.name;
            }

            bool isItem = !name.EndsWith("View") && !name.EndsWith("Win") && !name.EndsWith("Panel");
            var temp = new List<string>(path.Split('/'));
            int index = temp.IndexOf("AssetsPackage");
            if (temp.Count <= index + 3)
            {
                Log.Error("ui预制体路径应为  Assets/AssetsPackage/UI模块/UI子模块/Prefabs/预制体");
                return;
            }
            var dirPath = $"Assets/Scripts/Code/{generate_path}/{temp[index + 1]}/{temp[index + 2]}";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var csPath = $"{dirPath}/{name}.cs";
            bool exists = File.Exists(csPath);

            var infos = BuildNodeInfos(gos, rootTrans);

            StringBuilder strBuilder = new StringBuilder();
            StringBuilder tempBuilder = new StringBuilder();
            StringBuilder addListenerBuilder = new StringBuilder();
            strBuilder.AppendLine("using System.Collections;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("using System;")
                .AppendLine("using SuperScrollView;")
                .AppendLine("using UnityEngine;")
                .AppendLine("using UnityEngine.UI;\r\n");

            strBuilder.AppendLine("namespace TaoTie");
            strBuilder.AppendLine("{");

            strBuilder.AppendFormat("\tpublic class {0} : {1}, IOnCreate, IOnEnable\r\n", name,isItem?"UIBaseContainer":"UIBaseView");
            strBuilder.AppendLine("\t{");
            if (!isItem)
            {
                strBuilder.AppendFormat("\t\tpublic static string PrefabPath => \"{0}\";",
                        path.Replace(addressable_path, ""))
                    .AppendLine();
            }

            GenerateEntityChildCode(infos, strBuilder);
            strBuilder.AppendLine("\t\t\r\n");
            strBuilder.AppendLine("\t\t#region override");
            
            strBuilder.AppendLine("\t\tpublic void OnCreate()");
            strBuilder.AppendLine("\t\t{");
            GenerateSystemChildCode(infos, strBuilder, tempBuilder, addListenerBuilder);
            
            strBuilder.AppendLine("\t\t}");
            
            strBuilder.AppendLine("\t\tpublic void OnEnable()");
            strBuilder.AppendLine("\t\t{");
            strBuilder.Append(addListenerBuilder);
            strBuilder.AppendLine("\t\t}");
            strBuilder.AppendLine("\t\t#endregion");
            
            strBuilder.AppendLine();
            
            strBuilder.AppendLine("\t\t#region 事件绑定");
            strBuilder.Append(tempBuilder);
            strBuilder.AppendLine("\t\t#endregion");
            
            strBuilder.AppendLine("\t}");
            strBuilder.AppendLine("}");

            if (!forced_coverage && exists)
            {
                UnityEngine.Debug.LogWarning("已存在 " + csPath + "，不会覆盖生成，代码已复制到剪贴板。");
                EditorGUIUtility.systemCopyBuffer = strBuilder.ToString();
                return;
            }

            StreamWriter sw = new StreamWriter(csPath, false, Encoding.UTF8);
            sw.Write(strBuilder);
            sw.Flush();
            sw.Close();
            EditorGUIUtility.systemCopyBuffer = strBuilder.ToString();
        }

        private class NodeInfo
        {
            public string ModuleName;
            public string UpperName;
            public string Path;
            public Type ComponentType;
        }

        /// <summary>
        /// 遍历选中的节点，计算变量名（重名节点依次加index防重名）与相对路径
        /// </summary>
        static List<NodeInfo> BuildNodeInfos(GameObject[] gos, Transform root)
        {
            var infos = new List<NodeInfo>();
            var names = new HashSet<string>();
            foreach (var go in gos)
            {
                if (go == null) continue;
                string baseName = SanitizeName(go.name);
                if (string.IsNullOrEmpty(baseName)) continue;

                string moduleName = baseName;
                int i = 1;
                while (names.Contains(moduleName))
                {
                    moduleName = baseName + i;
                    i++;
                }
                names.Add(moduleName);

                string upperName = char.ToUpper(moduleName[0]) + moduleName.Substring(1);
                Type compType = null;
                foreach (var uiComponent in WidgetInterfaceList)
                {
                    if (go.GetComponent(uiComponent.Key) != null)
                    {
                        compType = uiComponent.Key;
                        break;
                    }
                }

                infos.Add(new NodeInfo
                {
                    ModuleName = moduleName,
                    UpperName = upperName,
                    Path = GetNodePath(go.transform, root),
                    ComponentType = compType,
                });
            }
            return infos;
        }

        static string SanitizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            var sb = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 计算选中节点相对根节点的路径，选中根节点时返回节点名
        /// </summary>
        static string GetNodePath(Transform node, Transform root)
        {
            var parts = new List<string>();
            var t = node;
            while (t != null && t != root)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            if (parts.Count == 0) return node.name;
            return string.Join("/", parts);
        }

        private static void GenerateEntityChildCode(List<NodeInfo> infos, StringBuilder strBuilder)
        {
            if (infos == null) return;
            foreach (var info in infos)
            {
                string widgetType = info.ComponentType != null ? WidgetInterfaceList[info.ComponentType] : "UIEmptyView";
                strBuilder.AppendFormat("\t\tpublic {0} {1};", widgetType, info.ModuleName)
                    .AppendLine();
            }
        }

        private static void GenerateSystemChildCode(List<NodeInfo> infos, StringBuilder strBuilder,
            StringBuilder tempBuilder, StringBuilder addListenerBuilder)
        {
            if (infos == null) return;
            foreach (var info in infos)
            {
                if (info.ComponentType == null)
                {
                    strBuilder.AppendFormat("\t\t\tthis.{0} = this.AddComponent<UIEmptyView>(\"{1}\");",
                            info.ModuleName, info.Path)
                        .AppendLine();
                    continue;
                }

                Type key = info.ComponentType;
                string widgetType = WidgetInterfaceList[key];
                strBuilder.AppendFormat("\t\t\tthis.{0} = this.AddComponent<{1}>(\"{2}\");",
                        info.ModuleName, widgetType, info.Path)
                    .AppendLine();

                if (key == typeof(Button) || key == typeof(PointerClick))
                {
                    addListenerBuilder.AppendFormat("\t\t\tthis.{0}.SetOnClick(OnClick{1});",
                            info.ModuleName, info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendFormat("\t\tpublic void OnClick{0}()", info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendLine("\t\t{").AppendLine();
                    tempBuilder.AppendLine("\t\t}");
                }
                else if (key == typeof(Toggle) || key == typeof(Dropdown))
                {
                    addListenerBuilder.AppendFormat("\t\t\tthis.{0}.SetOnValueChanged(SetOn{1}ValueChanged);",
                            info.ModuleName, info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendFormat("\t\tpublic void SetOn{0}ValueChanged({1} val)", info.UpperName, key == typeof(Toggle)?"bool":"int")
                        .AppendLine();
                    tempBuilder.AppendLine("\t\t{").AppendLine();
                    tempBuilder.AppendLine("\t\t}");
                }
                else if (key == typeof(SuperScrollView.LoopListView2))
                {
                    strBuilder.AppendFormat("\t\t\tthis.{0}.InitListView(0,Get{1}ItemByIndex);", info.ModuleName, info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendFormat("\t\tpublic LoopListViewItem2 Get{0}ItemByIndex(LoopListView2 listView, int index)", info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendLine("\t\t{");
                    tempBuilder.AppendLine("\t\t\treturn null;");
                    tempBuilder.AppendLine("\t\t}");
                }
                else if (key == typeof(SuperScrollView.LoopGridView))
                {
                    strBuilder.AppendFormat("\t\t\tthis.{0}.InitGridView(0,Get{1}ItemByIndex);", info.ModuleName, info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendFormat("\t\tpublic LoopGridViewItem Get{0}ItemByIndex(LoopGridView gridview, int index, int row, int column)", info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendLine("\t\t{");
                    tempBuilder.AppendLine("\t\t\treturn null;");
                    tempBuilder.AppendLine("\t\t}");
                }
                else if (key == typeof(CopyGameObject))
                {
                    strBuilder.AppendFormat("\t\t\tthis.{0}.InitListView(0,Get{1}ItemByIndex);", info.ModuleName, info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendFormat("\t\tpublic void Get{0}ItemByIndex(int index, GameObject obj)", info.UpperName)
                        .AppendLine();
                    tempBuilder.AppendLine("\t\t{").AppendLine();
                    tempBuilder.AppendLine("\t\t}");
                }
            }
        }
    }
}
