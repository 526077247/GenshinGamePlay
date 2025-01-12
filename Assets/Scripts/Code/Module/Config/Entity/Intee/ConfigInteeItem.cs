using LitJson.Extensions;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
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
        [OnValueChanged(nameof(UpdateIconPath))][JsonIgnore][BoxGroup("Icon")][NinoIgnore]
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
        [ReadOnly][NinoMember(5)][BoxGroup("Icon")]
        public string IconPath;
        
    }
}