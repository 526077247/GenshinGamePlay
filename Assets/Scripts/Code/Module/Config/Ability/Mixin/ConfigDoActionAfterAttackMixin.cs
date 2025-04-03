using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听攻击后
    /// </summary>
    [NinoType(false)][LabelText("攻击后DoAction")]
    public partial class ConfigDoActionAfterAttackMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}