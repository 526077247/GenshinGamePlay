using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 攻击前
    /// </summary>
    [NinoType(false)][LabelText("攻击前DoAction")]
    public partial class ConfigDoActionBeforeAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}