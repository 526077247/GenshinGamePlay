using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigInteeItem
    {
        [NinoMember(1)]
        public int LocalId;
        [NinoMember(2)]
        public string I18NKey;
        [NinoMember(3)]
        public string[] I18NParams;
        [NinoMember(4)] [LabelText("默认启用")]
        public bool DefaultEnable = true;
#if UNITY_EDITOR
        [OnValueChanged(nameof(UpdateIconPath))][JsonIgnore][BoxGroup("Icon")]
        public Sprite Icon;

        private void UpdateIconPath()
        {
            if (Icon == null)
            {
                IconPath = null;
                return;
            }

            var path = AssetDatabase.GetAssetPath(Icon);
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
                Icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetsPackage/" + IconPath);
                return;
            }
            Icon = null;
        }
#endif
        [ReadOnly][NinoMember(5)][BoxGroup("Icon")]
        public string IconPath;
        
    }
}