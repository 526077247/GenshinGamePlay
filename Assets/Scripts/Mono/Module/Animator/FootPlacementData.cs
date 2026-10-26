using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 单条肢体（左/右脚、左/右手）的落点 IK 参数与运行时状态。
    /// 作为 MecFootPlacer 的同物体组件存在，一条肢体一个；MecFootPlacer 按 <see cref="mFootID"/> 认领。
    /// 这里只存数据与纯函数，射线检测、骨盆补偿等解算全部在 MecFootPlacer 里。
    /// </summary>
    public class FootPlacementData : MonoBehaviour
    {
        public enum LimbID
        {
            LEFT_FOOT = 0,
            RIGHT_FOOT = 1,
            LEFT_HAND = 2,
            RIGHT_HAND = 3,
        }

        /// <summary>落点目标：FOOT = 脚掌锚点（世界坐标），TOE/HEEL = 相对脚掌的偏移向量。</summary>
        public enum Target
        {
            FOOT = 0,
            TOE = 1,
            HEEL = 2
        }

        [Header("Foot ID")] public LimbID mFootID = LimbID.LEFT_FOOT;

        [Space(10)] [Header("Foot Direction and Offset")]
        public Vector3 mForwardVector = new Vector3(0, 0, 1);

        public Vector3 mUpVector = new Vector3(0, 1, 0);

        [Space(10)] [Header("Foot Size Properties")]
        public float mFootOffsetDist = 0.5f;

        public float mFootLength = 0.22f;
        public float mFootHalfWidth = 0.05f;
        public float mFootHeight = 0.1f;

        [Space(10)] [Header("Foot Raycast Properties")]
        public float mTransitionTime = 0.2f;

        /// <summary>
        /// 射线全局延长量（米），三条射线（脚掌/趾尖/脚跟）都加。默认 0：本工程用下面更精确的
        /// <see cref="mFootRayDownhillSlack"/>，只在需要整体放宽探测深度时才动它。
        /// </summary>
        public float mExtraRayDistanceCheck = 0.0f;

        /// <summary>
        /// FOOT 射线的下坡余量（米）。脚踝被钉在「地面 + mFootHeight」后，FOOT 射程
        /// (mFootOffsetDist + mFootHeight) 刚好只够到脚底、零余量：平地上勉强命中，
        /// 下坡/下台阶时前脚正下方的地面天然低于脚底，就会常态 Miss。
        /// Miss 即 IK 权重归零、脚完全回到动画姿态 —— 坡面上表现为脚悬空、不贴坡。
        /// 只作用于 FOOT 射线；TOE/HEEL 射线已含 mFootLength 的天然余量。
        /// 过大会把「本应悬空」的脚吸到下方地面（下台阶拽脚），建议 0.05~0.1。
        /// </summary>
        public float mFootRayDownhillSlack = 0.08f;

        public float mFootRotationLimit = 45.0f;
        public bool mPlantFoot = true;
        public float mDisablePlantFromDistance = 0.30f;
        public float mDisablePlantFromAngle = 60.0f;

        [Space(10)] [Header("Draw Debugging Raycasts")]
        public bool mDrawDebugRay = false;

        // ---------- 运行时状态 ----------
        private readonly Vector3[] mTargetPos = new Vector3[3];
        private Vector3 mRotatedFwdVec;

        private bool mFootPlanted;
        private Vector3 mPlantedPos;
        private Quaternion mPlantedRot;

        /// <summary>
        /// 落脚瞬间的角色水平朝向（度）。落脚后脚不再跟着身体转，而保持落地时的朝向。
        /// 必须独立记录：不能从 PlantedRot（来自 GetIKRotation）反推 yaw，那条链是环。
        /// </summary>
        private float mPlantedYaw;

        private float mPlantBlendSpeed;
        private bool mPlantBlendOnTransition;

        // ---------- IK 权重与混合 ----------
        public float TargetFootWeight { get; set; }
        public float CurrentFootWeight { get; set; }
        public float GoalBlendSpeed { get; set; }
        public float PlantBlendFactor { get; set; }
        public float PlantBlendSpeed => mPlantBlendSpeed;
        public bool IsPlantOnTransition => mPlantBlendOnTransition;
        public bool IsPlantOnTransitionIn => mPlantBlendOnTransition && mPlantBlendSpeed > 0.0f;

        public Vector3 RotatedFwdVec => mRotatedFwdVec;

        // ---------- 落脚状态 ----------
        public bool FootPlanted
        {
            get { return mFootPlanted; }
            set
            {
                // 刚踏上地面：记录此刻身体朝向，作为这只脚整个支撑期的朝向基准
                if (value && !mFootPlanted) mPlantedYaw = GetBodyYaw();
                mFootPlanted = value;
            }
        }

        public Vector3 PlantedPos
        {
            get { return mPlantedPos; }
            set { mPlantedPos = value; }
        }

        public Quaternion PlantedRot
        {
            get { return mPlantedRot; }
            set { mPlantedRot = value; }
        }

        public void SetTargetPos(Target target, Vector3 pos)
        {
            mTargetPos[(int) target] = pos;
        }

        public Vector3 GetTargetPos(Target target)
        {
            return mTargetPos[(int) target];
        }

        /// <summary>
        /// 计算脚部朝向基准 <see cref="RotatedFwdVec"/>：角色水平朝向，已落脚时向「落脚瞬间的朝向」混合。
        ///
        /// 原实现取 mAnim.GetIKRotation(id).eulerAngles.y 当 yaw —— 这是正反馈环：
        /// 该值就是 MecFootPlacer 上一帧自己 SetIKRotation 写出去的结果，形成
        /// 「yaw → TOE/HEEL 探测方向 → 旋转目标 → yaw」的闭环；而且对带 pitch/roll 的旋转
        /// 取 eulerAngles.y 当 yaw 本身不成立（ZXY 分解下会被 x/z 分量带走）。
        /// 实测跑动中该 yaw 会漂到任意值（一秒内扫过 0.4°~179.3°、方向判断每秒翻转 2~6 次），
        /// 探测点跑到脚后跟一侧，再被 CheckForLimits 钳成假目标，脚踝于是被反复扭。
        /// 改用身体朝向做基准后，探测方向始终朝前，旋转构造连续、无分支。
        /// </summary>
        public void CalculateRotatedFwdVec()
        {
            float lYaw = GetBodyYaw();
            if (mFootPlanted && mPlantFoot && PlantBlendFactor > 0.0f)
            {
                lYaw = Mathf.LerpAngle(lYaw, mPlantedYaw, PlantBlendFactor);
            }

            mRotatedFwdVec = (Quaternion.Euler(0f, lYaw, 0f) * mForwardVector).normalized;
        }

        /// <summary>角色水平朝向（度，世界坐标）。脚部朝向与落脚朝向的共同基准。</summary>
        public float GetBodyYaw()
        {
            Vector3 lFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (lFwd.sqrMagnitude < 1e-6f) lFwd = transform.forward;
            return Mathf.Atan2(lFwd.x, lFwd.z) * Mathf.Rad2Deg;
        }

        public void EnablePlantBlend(float blendSpeed)
        {
            mPlantBlendSpeed = Mathf.Abs(blendSpeed);
            mPlantBlendOnTransition = true;
        }

        public void DisablePlantBlend(float blendSpeed)
        {
            mPlantBlendSpeed = -Mathf.Abs(blendSpeed);
            mPlantBlendOnTransition = true;
        }

        public void PlantBlendTransitionEnded()
        {
            mPlantBlendOnTransition = false;
        }

        /// <summary>
        /// 当前姿态是否仍允许这只脚保持「落脚」（不滑动）。
        /// 位移按 mUpVector 反向掩码（只比较垂直于 up 的水平分量），再判朝向差。
        /// </summary>
        public bool CanPlantFoot(Vector3 currentFootPos, Quaternion currentFootRot)
        {
            if (!mPlantFoot) return false;

            Vector3 lReverseUp = new Vector3(1 - mUpVector.x, 1 - mUpVector.y, 1 - mUpVector.z);
            Vector3 lDelta = mPlantedPos - currentFootPos;
            lDelta.x *= lReverseUp.x;
            lDelta.y *= lReverseUp.y;
            lDelta.z *= lReverseUp.z;

            if (lDelta.sqrMagnitude >= mDisablePlantFromDistance * mDisablePlantFromDistance) return false;

            return Quaternion.Angle(currentFootRot, mPlantedRot) < Mathf.Abs(mDisablePlantFromAngle);
        }
    }
}