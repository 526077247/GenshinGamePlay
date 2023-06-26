using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimeDilationClip : PlayableAsset, ITimelineClipAsset
{
    public TimeDilationBehaviour template = new TimeDilationBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Extrapolation | ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<TimeDilationBehaviour>.Create (graph, template);
    }
}
