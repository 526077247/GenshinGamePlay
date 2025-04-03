using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 受到攻击后
    /// </summary>
    [NinoType(false)][LabelText("当受到攻击后DoAction")]
    public partial class ConfigDoActionAfterBeAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}