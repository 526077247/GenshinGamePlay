using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace ScriptHotReload
{
    [InitializeOnLoad]
    public class HotReloadEditorToolbar
    {
        private static readonly Type kToolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

        private static ScriptableObject sCurrentToolbar;

        static HotReloadEditorToolbar()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (sCurrentToolbar == null)
            {
                UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(kToolbarType);
                sCurrentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
                if (sCurrentToolbar != null)
                {
                    FieldInfo root = sCurrentToolbar.GetType()
                        .GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement concreteRoot = root.GetValue(sCurrentToolbar) as VisualElement;
                    VisualElement toolbarZone = concreteRoot.Q("ToolbarZoneRightAlign");
                    VisualElement parent = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,

                            flexDirection = FlexDirection.Row,
                        }
                    };
                    IMGUIContainer container = new IMGUIContainer();
                    container.onGUIHandler += OnGuiBody;
                    parent.Add(container);
                    toolbarZone.Add(parent);
                }
            }
        }

        private static void OnGuiBody()
        {
            //自定义按钮加在此处
            GUILayout.BeginHorizontal();
            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button(new GUIContent("热重载", EditorGUIUtility.FindTexture("PlayButton"))))
                {
                    GenPatchAssemblies.DoGenPatchAssemblies();
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}