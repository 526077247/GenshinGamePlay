using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听间隔
    /// </summary>
    [ProtoContract][LabelText("间隔时间DoAction")]
    public partial class ConfigDoActionByTickMixin : ConfigAbilityMixin
    {
        [ProtoMember(4, IsRequired = true)][LabelText("每帧执行")]
        public bool EveryFrame = false;
        [ProtoMember(1, IsRequired = true)][LabelText("时间间隔(ms)")][MinValue(Define.MinRepeatedTimerInterval)][ShowIf("@!"+nameof(EveryFrame))]
        public int Interval = Define.MinRepeatedTimerInterval;
        
        [ProtoMember(2)][LabelText("添加后立即触发一次tick")]
        public bool TickFirstOnAdd;
        [ProtoMember(3)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}