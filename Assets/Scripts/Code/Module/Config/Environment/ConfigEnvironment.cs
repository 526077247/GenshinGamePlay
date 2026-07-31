using TaoTie.LitJson.Extensions;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigEnvironment
    {
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public ConfigBlender Enter;
        [ProtoMember(3)]
        public ConfigBlender Leave;
        [ProtoMember(4)]
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

        [ProtoMember(5)]
        public Color TintColor;

        [ProtoMember(6)][LabelText("使用直接光")]
        public bool UseDirLight;
        [ProtoMember(7, IsRequired = true)][LabelText("光照颜色")][ShowIf(nameof(UseDirLight))]
        public Color LightColor = Color.white;
        [ProtoMember(8, IsRequired = true)][LabelText("光照强度")][ShowIf(nameof(UseDirLight))][MinValue(0)]
        public float LightIntensity = 1;
        [ProtoMember(9, IsRequired = true)] [LabelText("光照方向")] [ShowIf(nameof(UseDirLight))]
        public Vector3 LightDir = new Vector3(50, -30, 0);
        [ProtoMember(10, IsRequired = true)] [LabelText("阴影类型")] [ShowIf(nameof(UseDirLight))]
        public LightShadows LightShadows = LightShadows.None;
    }
}