using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 属性值
    /// </summary>
    [NinoType(false)]
    public partial class NumericValue: BaseValue
    {
        [NinoMember(1)]
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetNumericFinalTypeId)+"()")]
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