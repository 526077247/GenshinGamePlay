using System;
using UnityEngine;
using UnityEngine.Playables;

public class TransformTweenMixerBehaviour : PlayableBehaviour
{
    bool m_FirstFrameHappened;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if(trackBinding == null)
            return;

        Vector3 defaultPosition = trackBinding.localPosition;
        Quaternion defaultRotation = trackBinding.localRotation;

        int inputCount = playable.GetInputCount ();

        float positionTotalWeight = 0f;
        float rotationTotalWeight = 0f;

        Vector3 blendedPosition = Vector3.zero;
        Quaternion blendedRotation = new Quaternion(0f, 0f, 0f, 0f);

        for (int i = 0; i < inputCount; i++)
        {
            ScriptPlayable<TransformTweenBehaviour> playableInput = (ScriptPlayable<TransformTweenBehaviour>)playable.GetInput (i);
            TransformTweenBehaviour input = playableInput.GetBehaviour ();
            

            float inputWeight = playable.GetInputWeight(i);

            if (!m_FirstFrameHappened)
            {
                input.startingPosition = defaultPosition;
                input.startingRotation = defaultRotation;
            }

            float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration ());
            float tweenProgress = input.EvaluateCurrentCurve(normalisedTime);

            
            positionTotalWeight += inputWeight;

            blendedPosition += Vector3.Lerp(input.startingPosition, input.endPosition, tweenProgress) * inputWeight;
            

            rotationTotalWeight += inputWeight;

            Quaternion desiredRotation = Quaternion.Lerp(input.startingRotation, input.endRotation, tweenProgress);
            desiredRotation = NormalizeQuaternion(desiredRotation);

            if (Quaternion.Dot (blendedRotation, desiredRotation) < 0f)
            {
                desiredRotation = ScaleQuaternion (desiredRotation, -1f);
            }

            desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);

            blendedRotation = AddQuaternions (blendedRotation, desiredRotation);
        }

        blendedPosition += defaultPosition * (1f - positionTotalWeight);
        Quaternion weightedDefaultRotation = ScaleQuaternion (defaultRotation, 1f - rotationTotalWeight);
        blendedRotation = AddQuaternions (blendedRotation, weightedDefaultRotation);

        trackBinding.localPosition = blendedPosition;
        trackBinding.localRotation = blendedRotation;
        
        m_FirstFrameHappened = true;
    }

    public override void OnPlayableDestroy (Playable playable)
    {
        m_FirstFrameHappened = false;
    }

    static Quaternion AddQuaternions (Quaternion first, Quaternion second)
    {
        first.w += second.w;
        first.x += second.x;
        first.y += second.y;
        first.z += second.z;
        return first;
    }

    static Quaternion ScaleQuaternion (Quaternion rotation, float multiplier)
    {
        rotation.w *= multiplier;
        rotation.x *= multiplier;
        rotation.y *= multiplier;
        rotation.z *= multiplier;
        return rotation;
    }

    static float QuaternionMagnitude (Quaternion rotation)
    {
        return Mathf.Sqrt ((Quaternion.Dot (rotation, rotation)));
    }

    static Quaternion NormalizeQuaternion (Quaternion rotation)
    {
        float magnitude = QuaternionMagnitude (rotation);

        if (magnitude > 0f)
            return ScaleQuaternion (rotation, 1f / magnitude);

        Debug.LogWarning ("Cannot normalize a quaternion with zero magnitude.");
        return Quaternion.identity;
    }
}