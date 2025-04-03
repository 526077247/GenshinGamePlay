using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 受到攻击前
    /// </summary>
    [NinoType(false)][LabelText("受到攻击前DoAction")]
    public partial class ConfigDoActionBeforeBeAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}