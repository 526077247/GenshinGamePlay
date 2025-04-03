using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听移除前
    /// </summary>
    [NinoType(false)][LabelText("ability或modify移除前DoAction")]
    public partial class ConfigDoActionBeforeRemoveMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}