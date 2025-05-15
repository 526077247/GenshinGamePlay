using TaoTie.LitJson.Extensions;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigEnvironment
    {
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
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
        [OnValueChanged(nameof(UpdateSkyCubePath))][BoxGroup("SkyCube")]
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

        [NinoMember(6)][LabelText("使用直接光")]
        public bool UseDirLight;
        [NinoMember(7)][LabelText("光照颜色")][ShowIf(nameof(UseDirLight))]
        public Color LightColor = Color.white;
        [NinoMember(8)][LabelText("光照强度")][ShowIf(nameof(UseDirLight))][MinValue(0)]
        public float LightIntensity = 1;
        [NinoMember(9)] [LabelText("光照方向")] [ShowIf(nameof(UseDirLight))]
        public Vector3 LightDir = new Vector3(50, -30, 0);
        [NinoMember(10)] [LabelText("阴影类型")] [ShowIf(nameof(UseDirLight))]
        public LightShadows LightShadows = LightShadows.None;
    }
}