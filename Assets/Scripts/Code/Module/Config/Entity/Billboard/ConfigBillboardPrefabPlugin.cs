using TaoTie.LitJson.Extensions;
using Nino.Core;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigBillboardPrefabPlugin: ConfigBillboardPlugin
    {
#if UNITY_EDITOR
        [OnValueChanged(nameof(UpdatePrefabPath))][BoxGroup("Prefab")]
        public GameObject Prefab;

        private void UpdatePrefabPath()
        {
            if (Prefab == null)
            {
                PrefabPath = null;
                return;
            }

            var path = AssetDatabase.GetAssetPath(Prefab);
            if (path.StartsWith("Assets/AssetsPackage/"))
            {
                PrefabPath = path.Replace("Assets/AssetsPackage/","");
            }
            else
            {
                PrefabPath = null;
            }
        }
        [Button("预览Prefab")][BoxGroup("Prefab")]
        private void Preview()
        {
            if (!string.IsNullOrEmpty(PrefabPath))
            {
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/" + PrefabPath);
                return;
            }
            Prefab = null;
        }
#endif
        [ReadOnly][NinoMember(5)][BoxGroup("Prefab")]
        public string PrefabPath;
    }
}