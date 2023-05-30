using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public partial class ConfigBillboardPlugin
    {
        [NinoMember(1)]
        public Vector3 Offset;
        
#if UNITY_EDITOR
        [OnValueChanged(nameof(UpdatePrefabPath))][JsonIgnore][BoxGroup("Prefab")]
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
        [ReadOnly][NinoMember(2)][BoxGroup("Prefab")]
        public string PrefabPath;
    }
}