using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// Mecanim Humanoid 的足部落地 IK。
    /// 每帧（<see cref="OnAnimatorIK"/>）对每条肢体：射线找地面接触点 → 构造脚部旋转 → 写 IK 位置/旋转/膝盖极向；
    /// 最后按实测腿长做骨盆垂直补偿（<see cref="RootPositioningCheck"/>）。
    /// 参数在 FootPlacementData 上（一条肢体一个组件），本组件只负责解算。
    /// </summary>
    public class MecFootPlacer : MonoBehaviour
    {
        public bool mAdjustPelvisVertically = false;
        public bool mDampPelvis = false;
        public float mMaxLegLength = 1.0f;
        public float mMinLegLength = 0.2f;
        public float mPelvisAdjustmentSpeed = 1.0f;

        /// <summary>
        /// 骨盆误差迟滞带（米）：腿长超过 mMaxLegLength 才「进入」骨盆下沉，
        /// 回落到 mMaxLegLength - mPelvisErrorDeadZone 才「退出」。
        /// 原实现进出共用同一阈值，跑动时腿长每步在阈值上下穿越（实测 0.83~0.93 vs 0.9），
        /// 目标误差就在 0 与几厘米之间横跳，骨盆跟着每步一沉一抬 —— 腿部闪烁的主要来源。
        /// </summary>
        public float mPelvisErrorDeadZone = 0.02f;

        /// <summary>
        /// 骨盆误差目标的一阶平滑速率（1/s，0=不平滑）。腿长测量本身有帧间噪声，
        /// 直接把它当目标会让骨盆跟着毫米级抖动；20/s ≈ 50ms 跟上真实坡度变化，只磨掉噪声。
        /// </summary>
        public float mPelvisTargetSmoothRate = 20f;

        /// <summary>
        /// 模型 pivot 补偿（米）：部分资产（如 KeQing）的模型根并不在脚底，而是浮在脚底上方一截。
        /// 实测该资产站立时脚踝离模型根只有 0.0455，而脚踝到脚底（= mFootHeight）是 0.1，
        /// 即脚底比模型根低 0.0545 —— 模型根贴地时脚掌有一半埋进地里；
        /// 反之若把脚踝按「地面 + mFootHeight」钉住，髋不动，腿就被压短 5.5cm，肉眼即「半蹲」。
        /// 取值为 mFootHeight − 动画站姿脚踝离模型根的高度，本资产 ≈ 0.0545。0 = 关闭。
        ///
        /// **消费方是 AnimatorMoveComponent，不是本组件**：必须抬「实体高度」而不是抬 Hips 骨骼。
        /// 实测把 Hips.position 直接在 OnAnimatorIK 里抬高是无效的 —— Unity 的 IK 回写按内部姿态
        /// 重新解算，手写位移既不参与解算也不保留（LateUpdate 里 hipY 仍是动画原值 0.871 而非 0.925），
        /// 于是脚踝目标被钉在地面、髋还在原处，腿恒被压短「mPelvisLift」那么多。
        /// 抬实体高度后模型根整体上移，动画脚踝自然落到「地面 + mFootHeight」，IK 退化为无操作，
        /// 腿既不拉伸也不屈曲，脚底同时贴地 —— 两个目标同时满足。
        /// 本组件只用它做射线射程补偿（见 <see cref="RayLiftCompensation"/>）。
        ///
        /// 注意：这属于资产缺陷的运行时兜底，根治应由美术把模型 pivot 归到脚底或修正动画脚踝高度。
        /// </summary>
        public float mPelvisLift = 0f;

        /// <summary>射线要忽略的层名，Start 时一次性折算成 LayerMask。</summary>
        public string[] mLayersToIgnore;

        /// <summary>
        /// 脚部射线射程补偿：抬升实体高度后，脚踝的动画位置整体上抬 mPelvisLift，
        /// 而射线的「起点 + 射程」是相对脚踝的定值 —— 于是脚踝下方可探测的深度被吃掉同样的量。
        /// FOOT 射线原本的射程 mFootOffsetDist + mFootHeight 刚好只够到脚底（零余量）：
        /// 抬升前脚踝离模型根只有 0.0455，尚余 0.054 余量；抬升后脚踝离地 0.1，余量归零，
        /// 平地上勉强命中，坡面/微起伏即常态 Miss —— Miss 时 IK 权重归零、脚完全回到动画姿态，
        /// 在坡面上就是「脚悬空、不贴坡」。把抬升量补进射程，覆盖范围与未抬升时的原始几何逐值等价。
        /// </summary>
        private float RayLiftCompensation
        {
            get { return mPelvisLift; }
        }

        private const float Epsilon = 0.005f;

        /// <summary>脚部 IK 旋转的最大角速度（度/秒）。只用于兜住接触面跳变（台阶、岩石棱）造成的单帧大跳，
        /// 正常跟随（贴坡、转向）单帧变化远小于此值，不受影响。</summary>
        private const float FootRotationMaxRate = 900f;

        /// <summary>四条肢体，下标与 AvatarIKGoal 枚举值一致（0=左脚 1=右脚 2=左手 3=右手）。</summary>
        private static readonly AvatarIKGoal[] AllGoals =
        {
            AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot, AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand
        };

        private FootPlacementData mLeftFoot;
        private FootPlacementData mRightFoot;
        private FootPlacementData mLeftHand;
        private FootPlacementData mRightHand;

        private Animator mAnim;
        private LayerMask mLayerMask = ~0;

        // 骨盆补偿状态
        private float mCurrentRootVertError;
        private float mTargetRootVertError;
        private float mCurrentPelvisSpeed;
        private bool mPelvisEngaged;
        private bool mRootPosRightFoot;

        /// <summary>每条肢体 × 每种目标（FOOT/TOE/HEEL）最后一次有效接触点，射线丢失时作为回退。</summary>
        private readonly Vector3[] mContactPos = new Vector3[12];

        /// <summary>脚部旋转限速的上一帧值（下标同 AvatarIKGoal）。</summary>
        private readonly Quaternion[] mFootRotApplied = new Quaternion[4];
        private readonly bool[] mFootRotAppliedValid = new bool[4];

        private readonly bool[] mLimbActive = { true, true, true, true };

        // 骨骼引用缓存：GetBoneTransform 是 native 查表，原先每帧调用十余次
        private Transform mHipsBone;
        private Transform mLeftUpperLegBone;
        private Transform mRightUpperLegBone;
        private Transform mLeftLowerLegBone;
        private Transform mRightLowerLegBone;

        // ============================ 生命周期 ============================

        void Start()
        {
            mAnim = GetComponent<Animator>();

            if (mAnim == null)
            {
                Debug.LogWarning("Mec Foot Placer will not work without Animator component. Please add one");
                return;
            }

            if (!mAnim.isHuman)
            {
                Debug.LogWarning("Mec Foot Placer only works with Mecanim humanoid rigs.");
            }

            // 层掩码与骨骼引用都是静态配置，缓存一次；避免每帧字符串查表与 native 骨骼查表
            BuildLayerMask();
            CacheBones();
            ResetPelvisState();

            FootPlacementData[] lLimbs = GetComponents<FootPlacementData>();
            for (int i = 0; i < lLimbs.Length; i++)
            {
                switch (lLimbs[i].mFootID)
                {
                    case FootPlacementData.LimbID.LEFT_FOOT: mLeftFoot = lLimbs[i]; break;
                    case FootPlacementData.LimbID.RIGHT_FOOT: mRightFoot = lLimbs[i]; break;
                    case FootPlacementData.LimbID.LEFT_HAND: mLeftHand = lLimbs[i]; break;
                    case FootPlacementData.LimbID.RIGHT_HAND: mRightHand = lLimbs[i]; break;
                }
            }
        }

        void OnAnimatorIK()
        {
            if (mAnim == null) return;

            for (int i = 0; i < AllGoals.Length; i++)
            {
                FootPlacement(AllGoals[i]);
            }

            RootPositioningCheck();
        }

        /// <summary>
        /// 骨盆误差最终换算成 Hips 骨骼的位移（只影响渲染姿态，不参与 IK 解算）。
        /// </summary>
        void LateUpdate()
        {
            if (mHipsBone == null || mCurrentRootVertError == 0f) return;

            FootPlacementData lFoot = mRootPosRightFoot ? mRightFoot : mLeftFoot;
            if (lFoot == null) return;

            mHipsBone.position -= lFoot.mUpVector * mCurrentRootVertError;
        }

        // ============================ 对外开关 ============================

        public void SetActive(AvatarIKGoal foot_id, bool active)
        {
            int i = (int)foot_id;
            if (i < 0 || i >= mLimbActive.Length) return;
            if (GetLimb(foot_id) == null) return;

            if (active && !mLimbActive[i]) ResetIKParams(foot_id);
            mLimbActive[i] = active;
        }

        public bool IsActive(AvatarIKGoal foot_id)
        {
            int i = (int)foot_id;
            return i >= 0 && i < mLimbActive.Length && mLimbActive[i];
        }

        // ============================ 单条肢体的解算 ============================

        private void FootPlacement(AvatarIKGoal foot_id)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null || !IsActive(foot_id)) return;

            lFoot.mUpVector.Normalize();
            lFoot.mForwardVector.Normalize();
            lFoot.CalculateRotatedFwdVec();

            lFoot.SetTargetPos(FootPlacementData.Target.FOOT, mAnim.GetIKPosition(foot_id));

            UpdateFootPlantBlendWeights(foot_id);
            FindContactPoints(foot_id);
            CheckForLimits(foot_id);
            CalculateIKGoalWeights(foot_id);

            /* 脚部旋转：连续构造 + 单帧限速，任何异常输入都不会在一帧内把脚踝扭过去 */
            mAnim.SetIKRotationWeight(foot_id, lFoot.CurrentFootWeight);
            mAnim.SetIKRotation(foot_id,
                ApplyFootRotationSlew(foot_id, lFoot.CurrentFootWeight, BuildFootRotation(lFoot)));

            /* 膝盖极向：只有双脚有 hint */
            if (foot_id == AvatarIKGoal.LeftFoot)
            {
                SetKneeHint(AvatarIKHint.LeftKnee, lFoot, mLeftUpperLegBone, mLeftLowerLegBone);
            }
            else if (foot_id == AvatarIKGoal.RightFoot)
            {
                SetKneeHint(AvatarIKHint.RightKnee, lFoot, mRightUpperLegBone, mRightLowerLegBone);
            }

            /* 脚部位置 */
            mAnim.SetIKPositionWeight(foot_id, lFoot.CurrentFootWeight);
            mAnim.SetIKPosition(foot_id,
                lFoot.GetTargetPos(FootPlacementData.Target.FOOT) + lFoot.mUpVector * lFoot.mFootHeight);

            UpdatePlantedState(lFoot, foot_id);
        }

        /// <summary>
        /// 维护「落脚」状态：权重归零时清标记并记录当前姿态；
        /// 有支撑权重、尚未落脚、且权重混合已完成时置为已落脚（此后脚不再随身体滑动）。
        /// </summary>
        private void UpdatePlantedState(FootPlacementData lFoot, AvatarIKGoal foot_id)
        {
            if (lFoot.CurrentFootWeight <= 0.0f)
            {
                lFoot.FootPlanted = false;
                lFoot.PlantedPos = mAnim.GetIKPosition(foot_id);
                lFoot.PlantedRot = mAnim.GetIKRotation(foot_id);
                return;
            }

            if (lFoot.FootPlanted) return;

            if (lFoot.IsPlantOnTransition)
            {
                // 过渡中：持续刷新落脚姿态，过渡结束那一刻即为最终落脚姿态
                lFoot.PlantedPos = mAnim.GetIKPosition(foot_id);
                lFoot.PlantedRot = mAnim.GetIKRotation(foot_id);
            }
            else
            {
                lFoot.FootPlanted = Mathf.Abs(lFoot.TargetFootWeight - lFoot.CurrentFootWeight) < Epsilon;
            }
        }

        /// <summary>射线找地面接触点，写 FOOT / TOE / HEEL 三组目标。</summary>
        private void FindContactPoints(AvatarIKGoal foot_id)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null) return;

            Vector3 lFootPos = mAnim.GetIKPosition(foot_id);
            Vector3 lUp = lFoot.mUpVector;
            Vector3 lFwd = lFoot.RotatedFwdVec;
            float lExtra = lFoot.mExtraRayDistanceCheck + RayLiftCompensation;

            // ---- 脚掌：脚踝正下方。射程补上实体抬升量（RayLiftCompensation）+ 下坡余量（mFootRayDownhillSlack）
            Vector3 lFootOffset = lUp * lFoot.mFootOffsetDist;
            float lRaySize = lFoot.mFootOffsetDist + lFoot.mFootHeight + lExtra + lFoot.mFootRayDownhillSlack;

            bool lFootHit = RaycastGround(lFootPos + lFootOffset, -lUp, lRaySize, out var lHit);
            if (lFootHit)
            {
                SetIKWeight(foot_id, 1, lFoot.mTransitionTime);
                Vector3 lResult = SettleOnPlanted(lFoot, lFootOffset, -lUp, lRaySize, lHit.point);
                lFoot.SetTargetPos(FootPlacementData.Target.FOOT, lResult);
                SetContactPos(foot_id, FootPlacementData.Target.FOOT, lResult);

                if (lFoot.mDrawDebugRay) Debug.DrawRay(lFootPos + lFootOffset, -lUp * lRaySize, Color.red);
            }
            else
            {
                SetIKWeight(foot_id, 0, lFoot.mTransitionTime);
                lFoot.SetTargetPos(FootPlacementData.Target.FOOT,
                    Vector3.Lerp(lFootPos, GetContactPos(foot_id, FootPlacementData.Target.FOOT),
                        lFoot.CurrentFootWeight));
            }

            // ---- 趾尖 / 脚跟：只用于构造脚部旋转，脚掌未命中时不再采信
            Quaternion lQuat90 = new Quaternion(0, 0.7071f, 0, 0.7071f);
            lQuat90.x *= lUp.x;
            lQuat90.y *= lUp.y;
            lQuat90.z *= lUp.z;

            lRaySize = lFoot.mFootOffsetDist + lFoot.mFootLength + lExtra;

            SampleFootEdge(foot_id, FootPlacementData.Target.TOE, lFoot, lFootHit,
                lFootOffset + lFwd * lFoot.mFootLength,
                lFwd * lFoot.mFootLength,
                -lUp, lRaySize, Color.blue);

            SampleFootEdge(foot_id, FootPlacementData.Target.HEEL, lFoot, lFootHit,
                lFootOffset + (lQuat90 * lFwd).normalized * lFoot.mFootHalfWidth,
                lQuat90 * lFwd * lFoot.mFootHalfWidth,
                -lUp, lRaySize, Color.green);
        }

        /// <summary>
        /// 趾尖/脚跟射线：命中则记录「命中点 − 脚掌目标」作为相对偏移；
        /// 未命中（或脚掌本身没接触）则按当前权重在「默认偏移」与「上次有效偏移」之间插值。
        /// </summary>
        private void SampleFootEdge(AvatarIKGoal foot_id, FootPlacementData.Target target,
            FootPlacementData lFoot, bool footHit, Vector3 originOffset, Vector3 fallbackOffset,
            Vector3 rayDir, float raySize, Color debugColor)
        {
            // 本次探测从「当前脚位」出发；落脚点的那次探测在 SettleOnPlanted 里单独做
            Vector3 lRayOrigin = mAnim.GetIKPosition(foot_id) + originOffset;

            if (footHit && RaycastGround(lRayOrigin, rayDir, raySize, out var lHit))
            {
                Vector3 lResult = SettleOnPlanted(lFoot, originOffset, rayDir, raySize, lHit.point);
                Vector3 lOffset = lResult - lFoot.GetTargetPos(FootPlacementData.Target.FOOT);
                lFoot.SetTargetPos(target, lOffset);
                SetContactPos(foot_id, target, lOffset);

                if (lFoot.mDrawDebugRay) Debug.DrawRay(lRayOrigin, rayDir * raySize, debugColor);
                return;
            }

            lFoot.SetTargetPos(target,
                Vector3.Slerp(fallbackOffset, GetContactPos(foot_id, target), lFoot.CurrentFootWeight));
        }

        /// <summary>
        /// 支撑期内把本次命中点向「落脚点处的地面」混合，抑制脚在支撑期内的滑动。
        /// 射线起点取落脚姿态的对应偏移，保证比较的是同一只脚在同一落点上的地面。
        /// </summary>
        private Vector3 SettleOnPlanted(FootPlacementData lFoot, Vector3 originOffset, Vector3 rayDir,
            float raySize, Vector3 hitPoint)
        {
            if (!lFoot.FootPlanted || lFoot.PlantBlendFactor <= Epsilon) return hitPoint;

            if (RaycastGround(lFoot.PlantedPos + originOffset, rayDir, raySize, out var lPlantedHit))
            {
                return Vector3.Lerp(hitPoint, lPlantedHit.point, lFoot.PlantBlendFactor);
            }

            return hitPoint;
        }

        private bool RaycastGround(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit)
        {
            // 触发器交互沿用工程全局设置：强制 Ignore 会改变「哪些碰撞体算落脚面」
            return PhysicsHelper.RaycastNonAlloc(origin, direction, out hit, maxDistance, mLayerMask);
        }

        private Vector3 GetContactPos(AvatarIKGoal foot_id, FootPlacementData.Target target)
        {
            int i = ContactIndex(foot_id, target);
            return i >= 0 ? mContactPos[i] : Vector3.zero;
        }

        private void SetContactPos(AvatarIKGoal foot_id, FootPlacementData.Target target, Vector3 pos)
        {
            int i = ContactIndex(foot_id, target);
            if (i >= 0) mContactPos[i] = pos;
        }

        /// <summary>接触点缓存的线性下标：肢体 0~3 × 目标 0~2。</summary>
        private static int ContactIndex(AvatarIKGoal foot_id, FootPlacementData.Target target)
        {
            int lLimb = (int)foot_id;
            return (lLimb >= 0 && lLimb < 4) ? lLimb * 3 + (int)target : -1;
        }

        /// <summary>IK 权重向目标线性混合。</summary>
        private void SetIKWeight(AvatarIKGoal foot_id, float target_weight, float transition_time)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null) return;

            // 目标没变时重算混合速度会把「已经走过的进度」清掉，只在真正变化时更新
            if (Mathf.Abs(target_weight - lFoot.TargetFootWeight) > Epsilon)
            {
                if (transition_time != 0f)
                {
                    lFoot.GoalBlendSpeed = (target_weight - lFoot.CurrentFootWeight) / transition_time;
                }
                else
                {
                    lFoot.GoalBlendSpeed = 0.1f;
                    lFoot.CurrentFootWeight = target_weight;
                }
            }

            lFoot.TargetFootWeight = target_weight;
        }

        /// <summary>按混合速度推进当前 IK 权重，收敛后直接对齐目标。</summary>
        private void CalculateIKGoalWeights(AvatarIKGoal foot_id)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null) return;

            float lSign = Mathf.Sign(lFoot.TargetFootWeight - lFoot.CurrentFootWeight);
            lFoot.CurrentFootWeight += lFoot.GoalBlendSpeed * Time.deltaTime;

            if (lSign * Mathf.Sign(lFoot.TargetFootWeight - lFoot.CurrentFootWeight) < 1f ||
                Mathf.Abs(lFoot.CurrentFootWeight - lFoot.TargetFootWeight) < Epsilon)
            {
                lFoot.CurrentFootWeight = lFoot.TargetFootWeight;
                return;
            }

            if (lFoot.CurrentFootWeight > 1f || lFoot.CurrentFootWeight < 0f)
            {
                lFoot.CurrentFootWeight = Mathf.Clamp(lFoot.TargetFootWeight, 0f, 1f);
            }
        }

        /// <summary>把趾尖/脚跟偏移钳进 mFootRotationLimit，避免脚掌俯仰/翻滚超出设定。</summary>
        private void CheckForLimits(AvatarIKGoal foot_id)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null) return;

            Vector3 lRotatedFwdVec = lFoot.RotatedFwdVec;
            Vector3 lUp = lFoot.mUpVector;
            float lTanLimit = Mathf.Tan(lFoot.mFootRotationLimit * Mathf.Deg2Rad);

            // 俯仰：趾尖
            Vector3 lToe = lFoot.GetTargetPos(FootPlacementData.Target.TOE);
            if (Vector3.Angle(lRotatedFwdVec, lToe) > lFoot.mFootRotationLimit)
            {
                Vector3 lExtra = lUp * lFoot.mFootLength * lTanLimit;
                if (Vector3.Angle(lUp, lToe) > 90f) lExtra = -lExtra;
                lFoot.SetTargetPos(FootPlacementData.Target.TOE, lRotatedFwdVec * lFoot.mFootLength + lExtra);
            }

            // 翻滚：脚跟
            Quaternion lQuat90 = new Quaternion(0, 0.7071f, 0, 0.7071f);
            lQuat90.x *= lUp.x;
            lQuat90.y *= lUp.y;
            lQuat90.z *= lUp.z;

            Vector3 lHeel = lFoot.GetTargetPos(FootPlacementData.Target.HEEL);
            if (Vector3.Angle(lQuat90 * lRotatedFwdVec, lHeel) > lFoot.mFootRotationLimit)
            {
                Vector3 lExtra = lUp * lFoot.mFootHalfWidth * lTanLimit;
                if (Vector3.Angle(lUp, lHeel) > 90f) lExtra = -lExtra;
                lFoot.SetTargetPos(FootPlacementData.Target.HEEL,
                    lQuat90 * lRotatedFwdVec * lFoot.mFootHalfWidth + lExtra);
            }
        }

        /// <summary>
        /// 支撑期混合：落脚时把接触点混合进落脚点，离地时衰减回动画脚位。
        /// 落脚判定同时看「位移超过 mDisablePlantFromDistance」与「朝向差超过 mDisablePlantFromAngle」。
        /// </summary>
        private void UpdateFootPlantBlendWeights(AvatarIKGoal foot_id)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null) return;

            bool lTimed = lFoot.mTransitionTime > Epsilon;
            float lSpeed = lTimed ? 1.0f / lFoot.mTransitionTime : 0f;

            if (lFoot.IsPlantOnTransition)
            {
                lFoot.PlantBlendFactor += lFoot.PlantBlendSpeed * Time.deltaTime;

                if (lFoot.IsPlantOnTransitionIn)
                {
                    if (lFoot.PlantBlendFactor >= 1f)
                    {
                        lFoot.PlantBlendFactor = 1f;
                        lFoot.PlantBlendTransitionEnded();
                    }
                }
                else if (lFoot.PlantBlendFactor < 0f)
                {
                    lFoot.PlantBlendFactor = 0f;
                    lFoot.PlantBlendTransitionEnded();
                }

                return;
            }

            bool lCanPlant = lFoot.FootPlanted &&
                             lFoot.CanPlantFoot(mAnim.GetIKPosition(foot_id), mAnim.GetIKRotation(foot_id));

            if (!lCanPlant)
            {
                if (lTimed) lFoot.DisablePlantBlend(lSpeed);
                else
                {
                    lFoot.PlantBlendFactor = 0f;
                    lFoot.PlantBlendTransitionEnded();
                }
            }
            else
            {
                if (lTimed) lFoot.EnablePlantBlend(lSpeed);
                else
                {
                    lFoot.PlantBlendFactor = 1f;
                    lFoot.PlantBlendTransitionEnded();
                }
            }
        }

        // ============================ 旋转构造 ============================

        /// <summary>
        /// 由「脚掌→趾尖」「脚掌→脚跟」两个接触点差向量，连续构造脚部世界旋转。
        ///
        /// 原实现用 FromToRotation 取 euler，并按 lAngle 是否 &gt;90 在两个相反的 FromToRotation 之间二选一：
        /// FromToRotation(a,b) 与 FromToRotation(b,a) 互为逆旋转，跨过 90° 时翻滚符号整体翻 180°；
        /// 而跑动中 lAngle 会来回穿越 90°（实测每秒翻转 2~6 次），脚踝就被反复扭。
        /// 现在改为「正交化 → LookRotation」：无分支、连续、不做 euler 分解。
        /// 退化或自相矛盾的输入（探测打到侧面/背面、方向接近平行）一律退回水平姿态：
        /// 宁可脚掌不贴坡，也绝不让脚踝扭。
        /// </summary>
        private Quaternion BuildFootRotation(FootPlacementData lFoot)
        {
            Vector3 lYawFwd = lFoot.RotatedFwdVec;
            if (lYawFwd.sqrMagnitude < 1e-6f) lYawFwd = transform.forward;
            Quaternion lFlat = Quaternion.LookRotation(lYawFwd.normalized, Vector3.up);

            Vector3 lToe = lFoot.GetTargetPos(FootPlacementData.Target.TOE);
            Vector3 lSide = lFoot.GetTargetPos(FootPlacementData.Target.HEEL);
            if (lToe.sqrMagnitude < 1e-4f || lSide.sqrMagnitude < 1e-6f) return lFlat;

            Vector3 lFwd = lToe.normalized;
            Vector3 lRight = Vector3.ProjectOnPlane(lSide, lFwd);
            if (lRight.sqrMagnitude < 1e-6f) return lFlat;
            lRight.Normalize();

            Vector3 lUp = Vector3.Cross(lFwd, lRight);
            // up 必须朝上；朝下说明两个方向自相矛盾（探针命中了侧面或背面）
            if (lUp.sqrMagnitude < 1e-6f || lUp.y <= 0.05f) return lFlat;
            lUp.Normalize();

            return Quaternion.LookRotation(lFwd, lUp);
        }

        /// <summary>
        /// 脚部 IK 旋转目标限速：单帧跳变超过 FootRotationMaxRate×dt 时平滑追上而不是直接写入。
        /// 权重为 0（脚在空中）时 IK 旋转不可见，直接对齐。
        /// </summary>
        private Quaternion ApplyFootRotationSlew(AvatarIKGoal foot_id, float weight, Quaternion target)
        {
            int i = (int)foot_id;
            if (i < 0 || i >= mFootRotApplied.Length) return target;

            if (weight <= 0.0f || !mFootRotAppliedValid[i])
            {
                mFootRotApplied[i] = target;
                mFootRotAppliedValid[i] = weight > 0.0f;
                return target;
            }

            mFootRotApplied[i] = Quaternion.RotateTowards(mFootRotApplied[i], target,
                FootRotationMaxRate * Time.deltaTime);
            return mFootRotApplied[i];
        }

        /// <summary>
        /// 膝盖极向（IK hint）构造：极向 = 角色前向在「髋→脚目标」平面上的投影。
        /// 目的：膝盖只朝前弯，不随坡度/屈膝幅度外翻，消除跑坡时的 O 型腿。
        ///
        /// 为什么废弃原来的 TOE + FOOT + mIKHintOffset + (knee - foot) 求和：
        /// ① mIKHintOffset 的横向分量（KeQing 左右各 ±0.1）会在髋外侧给极向加一个固定外翻偏置 ——
        ///    平地上屈膝不明显时看不出来，上坡时骨盆为够到坡下那只脚而整体下沉、双膝屈曲加大，
        ///    这个 ~14° 外翻被放大成肉眼可见的 O 型腿；
        /// ② 求和里的 TOE 实际存的是「趾尖命中点 − 脚掌命中点」的世界差，坡面上带竖直分量
        ///    （上坡趾尖比脚掌高、下坡反之），极向随坡度俯仰，膝盖朝向跟着地形抖；
        /// ③ 左右两只脚的极向基准还不一致（左脚用 GetIKPosition，右脚用骨骼位置）。
        /// 改成「腿轴 + 角色朝向」构造后，极向与坡度、屈膝幅度、接触点高度全部解耦。
        ///
        /// 退化处理：腿几乎与角色前向平行（极端前伸/后摆）时投影塌陷，退回动画自身的膝盖方向；
        /// 再塌陷则用世界竖直轴兜底，保证 hint 始终是有效方向。
        /// </summary>
        private void SetKneeHint(AvatarIKHint hint_id, FootPlacementData lFoot,
            Transform upperLeg, Transform lowerLeg)
        {
            mAnim.SetIKHintPositionWeight(hint_id, lFoot.CurrentFootWeight);

            if (upperLeg == null || lowerLeg == null) return;

            Vector3 lHipPos = upperLeg.position;
            // 与最终解算的脚位一致（接触点 + footHeight），极向才对准真正的腿轴
            Vector3 lFootPos = lFoot.GetTargetPos(FootPlacementData.Target.FOOT) + lFoot.mUpVector * lFoot.mFootHeight;
            Vector3 lHipToFoot = lFootPos - lHipPos;
            if (lHipToFoot.sqrMagnitude < 1e-6f) return;

            Vector3 lPoleDir = Vector3.ProjectOnPlane(transform.forward, lHipToFoot);
            if (lPoleDir.sqrMagnitude < 1e-4f)
            {
                lPoleDir = Vector3.ProjectOnPlane(lowerLeg.position - lHipPos, lHipToFoot);
            }

            if (lPoleDir.sqrMagnitude < 1e-4f)
            {
                lPoleDir = Vector3.ProjectOnPlane(Vector3.up, lHipToFoot);
            }

            if (lPoleDir.sqrMagnitude < 1e-4f) return;

            // hint 只提供方向（Unity 用它确定弯曲平面），取半腿长保证点位稳定
            float lHintDist = Mathf.Max(0.05f, lHipToFoot.magnitude * 0.5f);
            mAnim.SetIKHintPosition(hint_id, lHipPos + lPoleDir.normalized * lHintDist);
        }

        // ============================ 骨盆补偿 ============================

        private void RootPositioningCheck()
        {
            AnimateRootVertError();

            if (!mAdjustPelvisVertically || mLeftUpperLegBone == null || mRightUpperLegBone == null)
            {
                mTargetRootVertError = 0f;
                mCurrentPelvisSpeed = 0f;

                if (Mathf.Abs(mCurrentRootVertError) >= Epsilon)
                {
                    // 松开补偿时把偏移平滑放掉，避免髋部瞬移
                    Vector3 lUp = mRightFoot != null ? mRightFoot.mUpVector : Vector3.up;
                    ApplyRootVertError(lUp * mCurrentRootVertError);
                }

                return;
            }

            float lLegLenR = (mRightUpperLegBone.position - mAnim.GetIKPosition(AvatarIKGoal.RightFoot)).magnitude;
            float lLegLenL = (mLeftUpperLegBone.position - mAnim.GetIKPosition(AvatarIKGoal.LeftFoot)).magnitude;

            // 取更长的那条腿做判定，并用它的 up 方向施加偏移
            bool lUseRight = lLegLenR * lLegLenR >= lLegLenL * lLegLenL;
            float lLegLength = lUseRight ? lLegLenR : lLegLenL;
            FootPlacementData lFoot = lUseRight ? mRightFoot : mLeftFoot;
            mRootPosRightFoot = lUseRight;

            // 迟滞：进入用阈值 mMaxLegLength，退用阈值减去死区。同阈值进出会让目标在每次微小穿越上
            // 整体复位/重建，骨盆随之每步抖动。阈值不随 mPelvisLift 平移：pivot 补偿抬的是实体高度
            // （髋与地面一起上移），髋→脚踝的测量值不变。
            mPelvisEngaged = mPelvisEngaged
                ? lLegLength > mMaxLegLength - mPelvisErrorDeadZone
                : lLegLength > mMaxLegLength;

            if (mPelvisEngaged)
            {
                // 目标值再做一次一阶平滑：测量本身带帧间噪声，直接赋值会把毫米级抖动原样传给骨盆
                float lDesired = Mathf.Max(0f, lLegLength - mMaxLegLength);
                if (mPelvisTargetSmoothRate > 0f)
                {
                    float lK = 1f - Mathf.Exp(-mPelvisTargetSmoothRate * Time.deltaTime);
                    mTargetRootVertError = Mathf.Lerp(mTargetRootVertError, lDesired, lK);
                }
                else
                {
                    mTargetRootVertError = lDesired;
                }

                // 补偿后仍不能低于最小腿长（否则膝盖会反折），取更紧的那条腿
                if (mLeftFoot != null && mRightFoot != null)
                {
                    float lLeftError = Mathf.Max(0f, mMinLegLength -
                        (mLeftUpperLegBone.position -
                         (mAnim.GetIKPosition(AvatarIKGoal.LeftFoot) + mLeftFoot.mUpVector * mTargetRootVertError)).magnitude);
                    float lRightError = Mathf.Max(0f, mMinLegLength -
                        (mRightUpperLegBone.position -
                         (mAnim.GetIKPosition(AvatarIKGoal.RightFoot) + mRightFoot.mUpVector * mTargetRootVertError)).magnitude);

                    mTargetRootVertError -= Mathf.Max(lLeftError, lRightError);
                }
            }
            else
            {
                mTargetRootVertError = 0f;
                mCurrentPelvisSpeed = 0f;
            }

            ApplyRootVertError((lFoot != null ? lFoot.mUpVector : Vector3.up) * mCurrentRootVertError);
        }

        /// <summary>把骨盆垂直误差施加到四条肢体的 IK 目标上（脚/手一起走，保持同一支撑平面）。</summary>
        private void ApplyRootVertError(Vector3 delta)
        {
            for (int i = 0; i < AllGoals.Length; i++)
            {
                mAnim.SetIKPosition(AllGoals[i], mAnim.GetIKPosition(AllGoals[i]) + delta);
            }
        }

        /// <summary>骨盆误差向目标值靠拢（可选择带阻尼的缓入）。</summary>
        private void AnimateRootVertError()
        {
            float lErrorDiff = Mathf.Abs(mCurrentRootVertError - mTargetRootVertError);

            if (lErrorDiff < Epsilon)
            {
                mCurrentRootVertError = mTargetRootVertError;
                mCurrentPelvisSpeed = 0f;
                return;
            }

            float lSign = Mathf.Sign(mTargetRootVertError - mCurrentRootVertError);

            if (mDampPelvis)
            {
                if (lErrorDiff < mTargetRootVertError * 0.3f)
                {
                    mCurrentPelvisSpeed -= mPelvisAdjustmentSpeed * Time.deltaTime;
                    if (mCurrentPelvisSpeed < mPelvisAdjustmentSpeed * 0.5f)
                    {
                        mCurrentPelvisSpeed = mPelvisAdjustmentSpeed * 0.5f;
                    }

                    mCurrentRootVertError += lSign * mCurrentPelvisSpeed * Time.deltaTime;
                }
                else
                {
                    mCurrentRootVertError += lSign * mPelvisAdjustmentSpeed * Time.deltaTime;
                }
            }
            else
            {
                mCurrentRootVertError += lSign * mPelvisAdjustmentSpeed * Time.deltaTime;
                mCurrentPelvisSpeed = 0f;
            }

            // 越过目标就对齐，不让误差反向增长
            if (Mathf.Sign(mTargetRootVertError - mCurrentRootVertError) * lSign <= 0f)
            {
                mCurrentRootVertError = mTargetRootVertError;
            }
        }

        private void ResetPelvisState()
        {
            mPelvisEngaged = false;
            mCurrentRootVertError = 0f;
            mTargetRootVertError = 0f;
            mCurrentPelvisSpeed = 0f;
        }

        // ============================ 初始化 / 工具 ============================

        /// <summary>重新启用某条肢体前，把它的所有运行时状态清零，避免带着上一轮的接触点/权重回来。</summary>
        private void ResetIKParams(AvatarIKGoal foot_id)
        {
            FootPlacementData lFoot = GetLimb(foot_id);
            if (lFoot == null || mAnim == null) return;

            Vector3 lUp = lFoot.mUpVector;
            Quaternion lQuat90 = new Quaternion(0, 0.7071f, 0, 0.7071f);
            lQuat90.x *= lUp.x;
            lQuat90.y *= lUp.y;
            lQuat90.z *= lUp.z;

            Transform lBone = mAnim.GetBoneTransform(ToBone(foot_id));
            if (lBone == null) return;

            Vector3 lFootPos = lBone.position;
            Vector3 lFwd = lFoot.RotatedFwdVec;

            lFoot.TargetFootWeight = 0f;
            lFoot.CurrentFootWeight = 0f;
            lFoot.GoalBlendSpeed = 0f;
            lFoot.FootPlanted = false;

            SetContactPos(foot_id, FootPlacementData.Target.FOOT, lFootPos);
            SetContactPos(foot_id, FootPlacementData.Target.TOE,
                lUp * lFoot.mFootOffsetDist + lFwd * lFoot.mFootLength);
            SetContactPos(foot_id, FootPlacementData.Target.HEEL,
                lFootPos + lQuat90 * lFwd * lFoot.mFootHalfWidth);
        }

        /// <summary>
        /// 构建射线层掩码。原实现挂在 OnAnimatorIK 里每帧执行，每次都要做
        /// LayerMask.NameToLayer（字符串查表）与 Mathf.Pow；层配置是静态的，Start 算一次即可。
        /// </summary>
        private void BuildLayerMask()
        {
            mLayerMask = ~0;

            if (mLayersToIgnore == null) return;

            for (int i = 0; i < mLayersToIgnore.Length; i++)
            {
                if (string.IsNullOrEmpty(mLayersToIgnore[i])) continue;

                int lLayerNumber = LayerMask.NameToLayer(mLayersToIgnore[i]);
                if (lLayerNumber >= 0)
                {
                    mLayerMask &= ~(1 << lLayerNumber);
                }
            }
        }

        /// <summary>
        /// 缓存本组件用到的骨骼。GetBoneTransform 每次都是一次 native 查表，
        /// 原实现 RootPositioningCheck / SetKneeHint / LateUpdate 每帧合计调用十余次。
        /// </summary>
        private void CacheBones()
        {
            mHipsBone = GetBone(HumanBodyBones.Hips);
            mLeftUpperLegBone = GetBone(HumanBodyBones.LeftUpperLeg);
            mRightUpperLegBone = GetBone(HumanBodyBones.RightUpperLeg);
            mLeftLowerLegBone = GetBone(HumanBodyBones.LeftLowerLeg);
            mRightLowerLegBone = GetBone(HumanBodyBones.RightLowerLeg);
        }

        private Transform GetBone(HumanBodyBones bone)
        {
            if (mAnim == null || mAnim.avatar == null || !mAnim.isHuman) return null;
            return mAnim.GetBoneTransform(bone);
        }

        private FootPlacementData GetLimb(AvatarIKGoal foot_id)
        {
            switch (foot_id)
            {
                case AvatarIKGoal.LeftFoot: return mLeftFoot;
                case AvatarIKGoal.RightFoot: return mRightFoot;
                case AvatarIKGoal.LeftHand: return mLeftHand;
                case AvatarIKGoal.RightHand: return mRightHand;
                default: return null;
            }
        }

        private static HumanBodyBones ToBone(AvatarIKGoal foot_id)
        {
            switch (foot_id)
            {
                case AvatarIKGoal.LeftFoot: return HumanBodyBones.LeftFoot;
                case AvatarIKGoal.RightFoot: return HumanBodyBones.RightFoot;
                case AvatarIKGoal.LeftHand: return HumanBodyBones.LeftHand;
                default: return HumanBodyBones.RightHand;
            }
        }
    }
}
