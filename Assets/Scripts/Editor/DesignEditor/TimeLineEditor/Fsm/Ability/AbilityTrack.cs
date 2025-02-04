using UnityEngine.Timeline;
using System.Collections.Generic;

namespace TaoTie
{
    [TrackColor(0.875f, 0.5944853f, 0.1737132f)]
    [TrackClipType(typeof(AttachAbilityClip))]
    public class AbilityTrack: TrackAsset, IFsmSerializable
    {
        public void DoSerialize(List<ConfigFsmClip> clips)
        {
            foreach (var clip in GetClips())
            {
                if (clip.asset is AttachAbilityClip abilityClip)
                {
                    clips.Add(new ConfigAttachAbility
                    {
                        Length = (float)clip.duration,
                        StartTime = (float)clip.start,
                        AbilityName = abilityClip.AbilityName,
                        AddOnBreak = abilityClip.AddOnBreak,
                        RemoveOnOver = abilityClip.RemoveOnOver
                    });
                }
            }
        }
    }
}