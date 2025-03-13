using UnityEngine;
using System.Collections;

public class FootPlacementData : MonoBehaviour
{
    public enum LimbID
    {
        LEFT_FOOT = 0,
        RIGHT_FOOT = 1,
        LEFT_HAND = 2,
        RIGHT_HAND = 3,
    }

    public enum Target
    {
        FOOT = 0,
        TOE = 1,
        HEEL = 2
    }

    [Header("Foot ID")]
    public LimbID mFootID = LimbID.LEFT_FOOT;

    [Space(10)]

    [Header( "Foot Direction and Offest" )]
    public Vector3 mForwardVector = new Vector3(0, 0, 1);
    public Vector3 mIKHintOffset = new Vector3(0, 0, 0);
    public Vector3 mUpVector = new Vector3(0, 1, 0);

    [Space( 10 )]
    [Header( "Foot Size Properties" )]
    public float mFootOffsetDist = 0.5f;
    public float mFootLength = 0.22f;
    public float mFootHalfWidth = 0.05f;
    public float mFootHeight = 0.1f;

    [Space( 10 )]
    [Header( "Foot Raycast Properties" )]
    public float mTransitionTime = 0.2f;
    public float mExtraRayDistanceCheck = 0.0f;
    public float mFootRotationLimit = 45.0f;
    public bool mPlantFoot = true;
    public float mDisablePlantFromDistance = 0.30f;
    public float mDisablePlantFromAngle = 60.0f;

    [Space( 10 )]
    [Header( "Automatic Foot Stabilizer" )]
    public bool mSetExtraRayDistanceCheckAutomatically = false;
    public float mFootDistanceFromGroundThreshold = 0.1f;
    public float mExtraRayDistanceCheckMin = 0.0f;
    public float mExtraRayDistanceCheckMax = 2.0f;

    [Space( 10 )]
    [Header( "Draw Debuging Raycasts" )]
    public bool mDrawDebugRay = false;


    protected Vector3 mTargetPos = new Vector3(0.0f, 0.0f, 0.0f);
    protected Vector3 mTargetToePos = new Vector3(0.0f, 0.0f, 0.0f);
    protected Vector3 mTargetHeelPos = new Vector3(0.0f, 0.0f, 0.0f);

    protected Vector3 mRotatedFwdVec;
    protected Vector3 mRotatedIKHintOffset;

    protected float mTargetFootWeight = 0.0f;
    protected float mCurrentFootWeight = 0.0f;
    protected float mGoalBlendSpeed = 0.0f;
    protected float mPlantBlendFactor = 0.0f;
    protected float mFootPlantBlendSpeed;

    protected bool mFootPlantIsOnTransition = false;

    private Quaternion mFootPlantedRot;
    private Vector3 mFootPlantedPos;
    private Animator mAnim;
    private bool mFootPlanted = false;


    /*****************************************/
    public void SetTargetPos(Target target, Vector3 target_pos)
    {
        switch (target)
        {
            case Target.FOOT:
                mTargetPos = target_pos;
                break;

            case Target.TOE:
                mTargetToePos = target_pos;
                break;

            case Target.HEEL:
                mTargetHeelPos = target_pos;
                break;
        }
    }

    /*****************************************************/
    public Vector3 GetTargetPos(Target target)
    {
        switch (target)
        {
            case Target.FOOT:
                return mTargetPos;

            case Target.TOE:
                return mTargetToePos;

            case Target.HEEL:
                return mTargetHeelPos;
        }

        return Vector3.zero;
    }

    /**********************************/
    public void CalculateRotatedFwdVec()
    {
        AvatarIKGoal lFootID = AvatarIKGoal.LeftFoot;

        switch (mFootID)
        {
            case LimbID.RIGHT_FOOT:
                lFootID = AvatarIKGoal.RightFoot;
                break;
            case LimbID.LEFT_HAND:
                lFootID = AvatarIKGoal.LeftHand;
                break;
            case LimbID.RIGHT_HAND:
                lFootID = AvatarIKGoal.RightHand;
                break;
        }

        float lAngle = 0;
        Quaternion lYawRotation;

        lAngle = GetComponent<Animator>().GetIKRotation(lFootID).eulerAngles.y * Mathf.PI / 180;
        lYawRotation = new Quaternion(0, Mathf.Sin(lAngle * 0.5f), 0, Mathf.Cos(lAngle * 0.5f));


        if (mFootPlanted && mPlantFoot)
        {
            lAngle = mFootPlantedRot.eulerAngles.y * Mathf.PI / 180;
            lYawRotation = Quaternion.Slerp(lYawRotation, new Quaternion(0, Mathf.Sin(lAngle * 0.5f), 0, Mathf.Cos(lAngle * 0.5f)), mPlantBlendFactor);
        }

        mRotatedFwdVec = lYawRotation * mForwardVector.normalized;

    }

    /*******************************/
    public Vector3 GetRotatedFwdVec()
    {
        return mRotatedFwdVec;
    }

    /*******************************************/
    public void CalculateRotatedIKHint()
    {
        float lAngle = transform.rotation.eulerAngles.y * Mathf.PI / 180;
        Quaternion lYawRotation = new Quaternion(0, Mathf.Sin(lAngle * 0.5f), 0, Mathf.Cos(lAngle * 0.5f));

        mRotatedIKHintOffset = lYawRotation * mIKHintOffset;
    }

    /*******************************************/
    public Vector3 GetRotatedIKHint()
    {
        return mRotatedIKHintOffset;
    }

    /*******************************************/
    public void SetTargetFootWeight(float weight)
    {
        mTargetFootWeight = weight;
    }

    /*********************************/
    public float GetTargetFootWeight()
    {
        return mTargetFootWeight;
    }

    /*******************************************/
    public void SetCurrentFootWeight(float weight)
    {
        mCurrentFootWeight = weight;
    }

    /*********************************/
    public float GetCurrentFootWeight()
    {
        return mCurrentFootWeight;
    }

    /*******************************************/
    public void SetGoalBlendSpeed(float speed)
    {
        mGoalBlendSpeed = speed;
    }

    /*********************************/
    public float GetGoalBlendSpeed()
    {
        return mGoalBlendSpeed;
    }

    /*********************************/
    public float GetPlantBlendFactor()
    {
        return mPlantBlendFactor;
    }

    /*******************************************/
    public void SetPlantBlendFactor(float factor)
    {
        mPlantBlendFactor = factor;
    }

    /*********************************************/
    public void EnablePlantBlend(float blend_speed)
    {
        mFootPlantBlendSpeed = Mathf.Abs(blend_speed);
        mFootPlantIsOnTransition = true;
    }

    /******************************************************************/
    public void DisablePlantBlend(float blend_speed)
    {
        mFootPlantBlendSpeed = -Mathf.Abs(blend_speed);
        mFootPlantIsOnTransition = true;
    }

    /**********************************/
    public float GetFootPlantBlendSpeed()
    {
        return mFootPlantBlendSpeed;
    }

    /***********************************/
    public void PlantBlendTransitionEnded()
    {
        mFootPlantIsOnTransition = false;
    }

    /***********************************/
    public bool IsPlantOnTransition()
    {
        return mFootPlantIsOnTransition;
    }

    /***********************************/
    public bool IsPlantOnTransitionIn()
    {
        return IsPlantOnTransition() && (mFootPlantBlendSpeed > 0.0f);
    }

    /***********************************/
    public bool IsPlantOnTransitionOut()
    {
        return IsPlantOnTransition() && (mFootPlantBlendSpeed < 0.0f);
    }

    /**************************************/
    public void SetFootPlanted(bool planted)
    {
        mFootPlanted = planted;
    }

    /***************************/
    public bool GetFootPlanted()
    {
        return mFootPlanted;
    }

    /********************************************/
    public void SetPlantedPos(Vector3 planted_pos)
    {
        mFootPlantedPos = planted_pos;
    }

    /*****************************/
    public Vector3 GetPlantedPos()
    {
        return mFootPlantedPos;
    }

    /*****************************/
    public bool CanPlantFoot(Vector3 current_foot_pos, Quaternion currrent_foot_rotation)
    {
        if(mPlantFoot)
        {
            Vector3 lWeightedReverseUpVector = new Vector3(1 - mUpVector.x, 1 - mUpVector.y, 1 - mUpVector.z);
            Vector3 lMaskedFootPos = (mFootPlantedPos - current_foot_pos);
            lMaskedFootPos.x *= lWeightedReverseUpVector.x;
            lMaskedFootPos.y *= lWeightedReverseUpVector.y;
            lMaskedFootPos.z *= lWeightedReverseUpVector.z;

            if (lMaskedFootPos.sqrMagnitude < mDisablePlantFromDistance * mDisablePlantFromDistance)
            {
                float lDeltaDeg = Quaternion.Angle(currrent_foot_rotation, mFootPlantedRot);

                if(Mathf.Abs(lDeltaDeg) < Mathf.Abs(mDisablePlantFromAngle))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*******************************************/
    public void SetPlantedRot(Quaternion planted_rot)
    {
        mFootPlantedRot = planted_rot;
    }

    /*******************************/
    public Quaternion GetPlantedRot()
    {
        return mFootPlantedRot;
    }

    /*******************************/
    void Start()
    {
        mAnim = GetComponent<Animator>();
    }

    /*********************************************************************************************/
    protected bool IsLegCloseToGround(HumanBodyBones bone)
    {
        float lHeight = Vector3.Dot( gameObject.transform.position - mAnim.GetBoneTransform( bone ).position, mUpVector.normalized );

        if(Mathf.Abs(lHeight) < mFootDistanceFromGroundThreshold )
        {
            return true;
        }

        return false;
    }


    /***************************************************/
    void OnAnimatorIK()
    {
        //to check foot stability

        if ( !mSetExtraRayDistanceCheckAutomatically )
        {
            return;
        }

        HumanBodyBones lBone = HumanBodyBones.LeftFoot;

        switch (mFootID)
        {
            case LimbID.RIGHT_FOOT:
                lBone = HumanBodyBones.RightFoot;
                break;

            case LimbID.RIGHT_HAND:
                lBone = HumanBodyBones.RightHand;
                break;

            case LimbID.LEFT_HAND:
                lBone = HumanBodyBones.LeftHand;
                break;
        }

        if ( IsLegCloseToGround( lBone ) )
        {
            mExtraRayDistanceCheck = mExtraRayDistanceCheckMax;
        }
        else
        {
            mExtraRayDistanceCheck = mExtraRayDistanceCheckMin;
        }
    }
}
