#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using Sirenix.OdinInspector.Editor;

namespace Slate
{

    [CustomEditor(typeof(CutsceneGroup), true)]
    public class CutsceneGroupInspector : OdinEditor
    {

        private CutsceneGroup group {
            get { return (CutsceneGroup)target; }
        }

        public override void OnInspectorGUI() {
            GUI.enabled = group.root.currentTime == 0;
            base.OnInspectorGUI();
        }
    }
}

#endif