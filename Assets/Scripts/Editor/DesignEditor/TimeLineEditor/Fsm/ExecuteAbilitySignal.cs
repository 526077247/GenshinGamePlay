﻿using System;
using System.Collections.Generic;
using UnityEngine.Timeline;

namespace TaoTie
{
    [Serializable]
    public class ExecuteAbilitySignal: Marker,IFsmSerializableClip
    {
        public string AbilityName;
        public bool ExecuteOnBreak;
        public void DoSerialize(List<ConfigFsmClip> clips)
        {
            clips.Add(new ConfigExecuteAbility
            {
                Length = 0,
                StartTime = (float)this.time,
                AbilityName = this.AbilityName,
                ExecuteOnBreak = this.ExecuteOnBreak
            });
        }
    }
}