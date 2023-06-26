using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TransformTweenClip : PlayableAsset, ITimelineClipAsset
{
    [HideInInspector]
    public TransformTweenBehaviour template = new TransformTweenBehaviour ();
    public Vector3 startPosition;
    public Quaternion startRotation = Quaternion.identity;
    
    public Vector3 endPosition;
    public Quaternion endRotation = Quaternion.identity;

    public TransformTweenBehaviour.TweenType tweenType;

    [ShowIf(nameof(tweenType),TransformTweenBehaviour.TweenType.Custom)]
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TransformTweenBehaviour>.Create (graph, template);
        TransformTweenBehaviour clone = playable.GetBehaviour ();
        clone.startPosition = startPosition;
        clone.startRotation = startRotation;
        clone.endPosition = endPosition;
        clone.endRotation = endRotation;
        clone.tweenType = tweenType;
        clone.customCurve = curve;
        return playable;
    }
}