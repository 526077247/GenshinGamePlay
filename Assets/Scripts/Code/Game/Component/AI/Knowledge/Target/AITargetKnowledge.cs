using System;
using UnityEngine;

namespace TaoTie
{
    public class AITargetKnowledge: IDisposable
    {
        #region Fields and Properties

        /// <summary>
        /// 目标类型
        /// </summary>
        public AITargetType TargetType = AITargetType.InvalidTarget;

        /// <summary>
        /// 目标ID
        /// </summary>
        public long TargetID;

        /// <summary>
        /// 目标实体
        /// </summary>
        public Unit TargetEntity;

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 TargetPosition;

        /// <summary>
        /// 目标前方
        /// </summary>
        public Vector3 TargetForward;

        /// <summary>
        /// 目标方向
        /// </summary>
        public Vector3 TargetDirection;

        /// <summary>
        /// 目标距离
        /// </summary>
        public float TargetDistance;

        /// <summary>
        /// 目标水平距离
        /// </summary>
        public float TargetDistanceXZ;

        /// <summary>
        /// 目标垂直距离
        /// </summary>
        public float TargetDistanceY;

        /// <summary>
        /// 与目标关于y轴的相对角度
        /// </summary>
        public float TargetRelativeAngleYaw;

        /// <summary>
        /// 与目标关于y轴的相对角度绝对值
        /// </summary>
        public float TargetRelativeAngleYawAbs;

        /// <summary>
        /// 与目标关于x轴的相对角度
        /// </summary>
        public float TargetRelativeAnglePitch;

        /// <summary>
        /// 与目标关于x轴的相对角度绝对值
        /// </summary>
        public float TargetRelativeAnglePitchAbs;

        /// <summary>
        /// 目标是否在空中
        /// </summary>
        public bool TargetInAir;

        /// <summary>
        /// 目标是否被击杀
        /// </summary>
        public bool TargetKilled;

        /// <summary>
        /// 是否在目标的背后
        /// </summary>
        public bool IsSelfAtTargetBack;

        /// <summary>
        /// 是否被伙伴的目标覆盖
        /// </summary>
        public bool BuddyOverride;

        /// <summary>
        /// 技能锚点位置
        /// </summary>
        public Vector3 SkillAnchorPosition;

        /// <summary>
        /// 距离技能锚点距离
        /// </summary>
        public float SkillAnchorDistance;
        
        public Vector3? TargetLKP;
        
        /// <summary>
        /// 目标是否在防御区域内
        /// </summary>
        public bool TargetInDefendArea;
        /// <summary>
        /// 路径寻路状态
        /// </summary>
        public AITargetHasPathType HasPath;
        /// <summary>
        /// 目标是否没被遮挡物遮挡
        /// </summary>
        public bool HasLineOfSight;

        public static AITargetKnowledge Create()
        {
            return ObjectPool.Instance.Fetch<AITargetKnowledge>();
        }
        
        #endregion

        /// <summary>
        /// 清除目标
        /// </summary>
        /// <param name="clearType">目标类型</param>
        public void ClearTarget(AITargetType clearType)
        {
            switch (clearType)
            {
                case AITargetType.PointTarget:
                    TargetType = AITargetType.InvalidTarget;
                    TargetPosition = Vector3.zero;
                    HasPath = AITargetHasPathType.Invalid;
                    break;
                case AITargetType.EntityTarget:
                    TargetType = AITargetType.InvalidTarget;
                    HasPath = AITargetHasPathType.Invalid;
                    TargetID = 0;
                    TargetEntity = null;
                    break;
            }
        }

        private void SetEntityTargetInternal(long newTargetID, AIComponent ai)
        {
            if (TargetID != newTargetID)
            {
                this.TargetID = newTargetID;
                this.TargetEntity = ai.GetParent<Unit>().Parent.Get<Unit>(newTargetID);
                HasPath = AITargetHasPathType.Invalid;
            }
        }

        /// <summary>
        /// 设置实体目标
        /// </summary>
        /// <param name="targetSource">目标来源</param>
        /// <param name="newTargetID">目标ID</param>
        /// <param name="ai">AI组件</param>
        public void SetEntityTarget(AITargetSource targetSource, long newTargetID, AIComponent ai)
        {
            TargetType = AITargetType.EntityTarget;
            if (targetSource == AITargetSource.Threat)
            {
                SetEntityTargetInternal(newTargetID, ai);
            }
        }

        /// <summary>
        /// 设置位置目标
        /// </summary>
        /// <param name="pos">位置坐标</param>
        public void SetPointTarget(Vector3 pos)
        {
            TargetType = AITargetType.PointTarget;
            TargetPosition = pos;
            HasPath = AITargetHasPathType.Invalid;
        }

        public void Dispose()
        {
            TargetID = 0;
            TargetPosition =Vector3.zero;
            TargetForward =Vector3.zero;
            TargetDirection=Vector3.zero;
            TargetDistance = 0;
            TargetDistanceXZ = 0;
            TargetDistanceY = 0;
            TargetRelativeAngleYaw = 0;
            TargetRelativeAngleYawAbs = 0;
            TargetRelativeAnglePitch = 0;
            TargetRelativeAnglePitchAbs = 0;
            TargetInAir = false;
            TargetKilled = false;
            IsSelfAtTargetBack = false;
            BuddyOverride = false;
            SkillAnchorPosition = Vector3.zero;
            SkillAnchorDistance = 0;
            TargetLKP = null;
            TargetInDefendArea = false;
            HasPath = AITargetHasPathType.Invalid;
            HasLineOfSight = false;
            ObjectPool.Instance.Recycle(this);
        }
    }
}