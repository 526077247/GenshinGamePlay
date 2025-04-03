using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听调用Execute方法
    /// </summary>
    [NinoType(false)][LabelText("ability或modify调用执行时DoAction")]
    public partial class ConfigDoActionByExecuteMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}