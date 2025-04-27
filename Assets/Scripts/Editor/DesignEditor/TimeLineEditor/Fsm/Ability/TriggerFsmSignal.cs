using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Timeline;

namespace TaoTie
{
    [Serializable]
    public class TriggerFsmSignal: Marker,IFsmSerializable
    {
        public string TriggerId;
        [LabelText("当还未开始时被打断是否触发")]
        public bool TriggerOnBreak;
        public void DoSerialize(List<ConfigFsmClip> clips)
        {
            clips.Add(new ConfigTriggerClip()
            {
                Length = 0,
                StartTime = (float)this.time,
                TriggerId = this.TriggerId,
                TriggerOnBreak = this.TriggerOnBreak
            });
        }
    }
}