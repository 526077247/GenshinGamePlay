using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听添加后
    /// </summary>
    [NinoType(false)][LabelText("ability或modify添加后DoAction")]
    public partial class ConfigDoActionAfterAddMixin : ConfigAbilityMixin
    {
        [NinoMember(1)]
        public ConfigAbilityAction[] Actions;
    }
}