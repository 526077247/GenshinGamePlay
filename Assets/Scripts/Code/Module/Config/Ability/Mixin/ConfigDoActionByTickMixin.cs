using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听间隔
    /// </summary>
    [NinoType(false)][LabelText("间隔时间DoAction")]
    public partial class ConfigDoActionByTickMixin : ConfigAbilityMixin
    {
        [NinoMember(1)][LabelText("时间间隔(ms)")][MinValue(1)]
        public uint Interval = 1;
        
        [NinoMember(2)][LabelText("添加后立即触发一次tick")]
        public bool TickFirstOnAdd;
        [NinoMember(3)]
        public ConfigAbilityAction[] Actions;
    }
}