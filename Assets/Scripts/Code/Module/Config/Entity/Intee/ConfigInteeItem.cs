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
    public partial class ConfigInteeItem
    {
        [ProtoMember(1)]
        public int LocalId;
        [ProtoMember(2)]
        public I18NKey I18NKey;
        [ProtoMember(3)]
        public string[] I18NParams;
        [ProtoMember(4, IsRequired = true)] [LabelText("默认启用")]
        public bool DefaultEnable = true;
#if UNITY_EDITOR
        [OnValueChanged(nameof(UpdateIconPath))][BoxGroup("Icon")][ProtoIgnore]
        public Sprite Icon;

        private void UpdateIconPath()
        {
            if (Icon == null)
            {
                IconPath = null;
                return;
            }

            var path = UnityEditor.AssetDatabase.GetAssetPath(Icon);
            if (path.StartsWith("Assets/AssetsPackage/"))
            {
                IconPath = path.Replace("Assets/AssetsPackage/","");
            }
            else
            {
                IconPath = null;
            }
        }
        [Button("预览Icon")][BoxGroup("Icon")]
        private void Preview()
        {
            if (!string.IsNullOrEmpty(IconPath))
            {
                Icon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetsPackage/" + IconPath);
                return;
            }
            Icon = null;
        }
#endif
        [ReadOnly][ProtoMember(5)][BoxGroup("Icon")]
        public string IconPath;
        
    }
}