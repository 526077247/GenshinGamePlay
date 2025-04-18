﻿using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 监听间隔
    /// </summary>
    [NinoType(false)][LabelText("间隔时间DoAction")]
    public partial class ConfigDoActionByTickMixin : ConfigAbilityMixin
    {
        [NinoMember(4)][LabelText("每帧执行")]
        public bool EveryFrame = false;
        [NinoMember(1)][LabelText("时间间隔(ms)")][MinValue(Define.MinRepeatedTimerInterval)][ShowIf("@!"+nameof(EveryFrame))]
        public int Interval = Define.MinRepeatedTimerInterval;
        
        [NinoMember(2)][LabelText("添加后立即触发一次tick")]
        public bool TickFirstOnAdd;
        [NinoMember(3)][LabelText("Actions:初始Action目标(Target)为Applier(Modify或Ability持有者)")]
        public ConfigAbilityAction[] Actions;
    }
}