using ProtoBuf;
#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
#endif
namespace TaoTie
{
    /// <summary>
    /// 属性值
    /// </summary>
    [ProtoContract]
    public partial class NumericValue: BaseValue
    {
        [ProtoMember(1)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericFinalTypeId)+"()")]
#endif
        public int Key;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            var numc = entity.GetComponent<NumericComponent>();
            if (numc != null)
            {
                return numc.GetAsFloat(Key);
            }
            Log.Error($"获取{Key}时，未找到NumericComponent组件");
            return 0;
        }
    }
}