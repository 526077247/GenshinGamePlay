using System.Reflection;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class OpenView: ConfigAbilityAction
    {
#if UNITY_EDITOR
        private void RefreshViewPath()
        {
            var baseView = TypeInfo<UIBaseView>.Type;
            var type = baseView.Assembly.GetType(baseView.Namespace + "." + ViewType);
            if (type != null)
            {
                BindingFlags flag = BindingFlags.Static | BindingFlags.Public;
                var props = type.GetProperties(flag);
                for (int j = 0; j < props.Length; j++)
                {
                    var str = props[j].GetValue(null) as string;
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/AssetsPackage/" + str);
                    if (prefab != null)
                    {
                        ViewPath = str;
                        break;
                    }
                }
                var fields = type.GetFields(flag);
                for (int j = 0; j < fields.Length; j++)
                {
                    var str = fields[j].GetValue(null) as string;
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/AssetsPackage/" + str);
                    if (prefab != null)
                    {
                        ViewPath = str;
                        break;
                    }
                }
            }
        }
        [Sirenix.OdinInspector.ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAllView)+"()",AppendNextDrawer = true)]
        [Sirenix.OdinInspector.OnValueChanged(nameof(RefreshViewPath))]
#endif
        [NinoMember(10)]
        public string ViewType;
        [NinoMember(11)]
        public string ViewPath;
        [NinoMember(12)]
        public UILayerNames Layer = UILayerNames.NormalLayer;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            UIManager.Instance.OpenWindow(ViewType, ViewPath, Layer).Coroutine();
        }
    }
}