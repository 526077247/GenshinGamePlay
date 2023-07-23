using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TransformTweenBehaviour : PlayableBehaviour
{
    public enum TweenType
    {
        Linear,
        Deceleration,
        Harmonic,
        Custom,
    }
    public Vector3 startPosition;
    public Quaternion startRotation = Quaternion.identity;
    
    public Vector3 endPosition;
    public Quaternion endRotation = Quaternion.identity;
    
    public TweenType tweenType;
    public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    public Vector3 startingPosition;
    public Quaternion startingRotation = Quaternion.identity;

    AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    AnimationCurve m_DecelerationCurve = new AnimationCurve
    (
        new Keyframe(0f, 0f, -k_RightAngleInRads, k_RightAngleInRads),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    AnimationCurve m_HarmonicCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    const float k_RightAngleInRads = Mathf.PI * 0.5f;

    public override void PrepareFrame (Playable playable, FrameData info)
    {
        startingPosition = startPosition;
        startingRotation = startRotation;
    }

    public float EvaluateCurrentCurve (float time)
    {
        if (tweenType == TweenType.Custom && !IsCustomCurveNormalised ())
        {
            Debug.LogError("Custom Curve is not normalised.  Curve must start at 0,0 and end at 1,1.");
            return 0f;
        }
        
        switch (tweenType)
        {
            case TweenType.Linear:
                return m_LinearCurve.Evaluate (time);
            case TweenType.Deceleration:
                return m_DecelerationCurve.Evaluate (time);
            case TweenType.Harmonic:
                return m_HarmonicCurve.Evaluate (time);
            default:
                return customCurve.Evaluate (time);
        }
    }

    bool IsCustomCurveNormalised ()
    {
        if (!Mathf.Approximately (customCurve[0].time, 0f))
            return false;
        
        if (!Mathf.Approximately (customCurve[0].value, 0f))
            return false;
        
        if (!Mathf.Approximately (customCurve[customCurve.length - 1].time, 1f))
            return false;
        
        return Mathf.Approximately (customCurve[customCurve.length - 1].value, 1f);
    }
}