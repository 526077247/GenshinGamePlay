#if UNITY_EDITOR
using System.Collections.Generic;
using Slate;

namespace TaoTie
{
    [Attachable(typeof(FSMTrack))]
    public class ExecuteAbilityClip: ActionClip,ISerializableClip
    {
        public string AbilityName;

        public void DoSerialize(List<ConfigFsmClip> clips)
        {
            clips.Add(new ConfigExecuteAbility
            {
                Length = this.length,
                StartTime = this.startTime,
                AbilityName = this.AbilityName
            });
        }
    }
}
#endif