using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigEnvironment
    {
#if UNITY_EDITOR
        [NinoMember(0)][LabelText("策划备注")]
        public int Remarks;
#endif
        [NinoMember(1)]
        public int Id;
        [NinoMember(2)]
        public ConfigBlender Enter;
        [NinoMember(3)]
        public ConfigBlender Leave;
        [NinoMember(4)]
        public string SkyCubePath;
#if UNITY_EDITOR
        [OnValueChanged(nameof(UpdateSkyCubePath))][JsonIgnore][BoxGroup("SkyCube")]
        public Cubemap SkyCube;

        private void UpdateSkyCubePath()
        {
            if (SkyCube == null)
            {
                SkyCubePath = null;
                return;
            }

            var path = UnityEditor.AssetDatabase.GetAssetPath(SkyCube);
            if (path.StartsWith("Assets/AssetsPackage/"))
            {
                SkyCubePath = path.Replace("Assets/AssetsPackage/","");
            }
            else
            {
                SkyCubePath = null;
            }
        }
        [Button("预览SkyCube")][BoxGroup("SkyCube")]
        private void PreviewSkyCube()
        {
            if (!string.IsNullOrEmpty(SkyCubePath))
            {
                SkyCube = UnityEditor.AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/AssetsPackage/" + SkyCubePath);
                return;
            }
            SkyCube = null;
        }
#endif

        [NinoMember(5)]
        public Color TintColor;

    }
}