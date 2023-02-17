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
        public AITargetType targetType = AITargetType.InvalidTarget;

        /// <summary>
        /// 目标ID
        /// </summary>
        public int targetID;

        /// <summary>
        /// 目标实体
        /// </summary>
        public Unit targetEntity;
        public CombatComponent targetAvatarCombat;
        public AbilityComponent targetAbilityPlugin;
        

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 targetPosition;

        /// <summary>
        /// 目标前方
        /// </summary>
        public Vector3 targetForward;

        /// <summary>
        /// 目标方向
        /// </summary>
        public Vector3 targetDirection;

        /// <summary>
        /// 目标距离
        /// </summary>
        public float targetDistance;

        /// <summary>
        /// 目标水平距离
        /// </summary>
        public float targetDistanceXZ;

        /// <summary>
        /// 目标垂直距离
        /// </summary>
        public float targetDistanceY;

        /// <summary>
        /// 与目标关于y轴的相对角度
        /// </summary>
        public float targetRelativeAngleYaw;

        /// <summary>
        /// 与目标关于y轴的相对角度绝对值
        /// </summary>
        public float targetRelativeAngleYawAbs;

        /// <summary>
        /// 与目标关于x轴的相对角度
        /// </summary>
        public float targetRelativeAnglePitch;

        /// <summary>
        /// 与目标关于x轴的相对角度绝对值
        /// </summary>
        public float targetRelativeAnglePitchAbs;

        /// <summary>
        /// 目标是否在空中
        /// </summary>
        public bool targetInAir;

        /// <summary>
        /// 目标是否被击杀
        /// </summary>
        public bool targetKilled;

        /// <summary>
        /// 是否在目标的背后
        /// </summary>
        public bool isSelfAtTargetBack;

        /// <summary>
        /// 是否被伙伴的目标覆盖
        /// </summary>
        public bool buddyOverride;

        /// <summary>
        /// 技能锚点位置
        /// </summary>
        public Vector3 skillAnchorPosition;

        /// <summary>
        /// 距离技能锚点距离
        /// </summary>
        public float skillAnchorDistance;
        public Vector3? targetLKP;
        
        public bool targetInDefendArea;
        public AITargetHasPathType hasPath;
        public float hasPathTick;
        public bool hasLineOfSight;
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
                    targetType = AITargetType.InvalidTarget;
                    targetPosition = Vector3.zero;
                    break;
                case AITargetType.EntityTarget:
                    targetType = AITargetType.InvalidTarget;
                    targetID = 0;
                    targetEntity = null;
                    break;
            }
        }

        private void SetEntityTargetInternal(int newTargetID, AIComponent ai)
        {
            this.targetID = newTargetID;
            this.targetEntity = ai.GetParent<Unit>();
        }

        /// <summary>
        /// 设置实体目标
        /// </summary>
        /// <param name="targetSource">目标来源</param>
        /// <param name="newTargetID">目标ID</param>
        /// <param name="ai">AI组件</param>
        public void SetEntityTarget(AITargetSource targetSource, int newTargetID, AIComponent ai)
        {
            targetType = AITargetType.EntityTarget;
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
            targetType = AITargetType.PointTarget;
            targetPosition = pos;
        }

        public void Dispose()
        {
            
        }
    }
}