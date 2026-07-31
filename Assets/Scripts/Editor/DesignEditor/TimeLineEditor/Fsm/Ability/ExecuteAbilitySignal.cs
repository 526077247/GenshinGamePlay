using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine.Timeline;

namespace TaoTie
{
    [Serializable]
    public class ExecuteAbilitySignal: Marker,IFsmSerializable
    {
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
        public string AbilityName;
        [LabelText("当还未开始时被打断是否执行")]
        public bool ExecuteOnBreak;
        public void DoSerialize(List<ConfigFsmClip> clips)
        {
            clips.Add(new ConfigExecuteAbilityClip
            {
                Length = 0,
                StartTime = (float)this.time,
                AbilityName = this.AbilityName,
                ExecuteOnBreak = this.ExecuteOnBreak
            });
        }
    }
}