using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class FacingMoveInfo : MoveInfoBase
    {
        private long nextTickPickDirection;
        // private long nextTickBackRaycast;
        // private long nextTickMoveDirObstacleCheck;
        private bool isBackClear;
        private MotionDirection currentMoveDirection;

        public static FacingMoveInfo Create()
        {
            return ObjectPool.Instance.Fetch<FacingMoveInfo>();
        }

        public override void Dispose()
        {
            currentMoveDirection = MotionDirection.Idle;
            nextTickPickDirection = 0;
            // nextTickBackRaycast = 0;
            // nextTickMoveDirObstacleCheck = 0;
            isBackClear = false;
            ObjectPool.Instance.Recycle(this);
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            isBackClear = true;
            currentMoveDirection = MotionDirection.Idle;
            nextTickPickDirection = 0;
            // nextTickBackRaycast = 0;
            // nextTickMoveDirObstacleCheck = 0;
            CreateNewTask(taskHandler, aiKnowledge);
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            if(taskHandler.CurrentState == LocoTaskState.Running)
                taskHandler.CurrentState = LocoTaskState.Interrupted;
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            CreateNewTask(taskHandler, aiKnowledge);
        }


        private void CreateNewTask(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            FacingMoveType moveType = (aiKnowledge.Mover?.Config as ConfigAnimatorMove)?.FacingMove ?? FacingMoveType.FourDirection;
            MotionDirection dir = MotionDirection.Idle;
            bool needUpdate = false;
            var dis = aiKnowledge.TargetKnowledge.TargetDistance;
            if (dis < aiKnowledge.FacingMoveTactic.Data.RangeMin 
                && moveType!= FacingMoveType.LeftRight 
                && moveType!= FacingMoveType.ForwardOnly)
            {
                dir =  MotionDirection.Backward;
                needUpdate = true;
            }
            if (dis > aiKnowledge.FacingMoveTactic.Data.RangeMax
                && moveType!= FacingMoveType.ForwardOnly)
            {
                dir =  MotionDirection.Forward;
                needUpdate = true;
            }
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            if (!needUpdate && timeNow < nextTickPickDirection) return;
            
            ConfigAIFacingMoveData data = aiKnowledge.FacingMoveTactic.Data;
            
            bool canLR = CheckLRHitWall(aiKnowledge, data.ObstacleDetectRange);
            var during = Random.Range(data.RestTimeMin, data.RestTimeMax);
            nextTickPickDirection = GameTimerManager.Instance.GetTimeNow() + during;
            if(!needUpdate)
                dir = GetNewMoveDirection(data.FacingMoveWeight, moveType);
            if (!canLR && (dir == MotionDirection.Left || dir == MotionDirection.Right))
            {
                dir = MotionDirection.Idle;
            }

            if (!isBackClear && dir == MotionDirection.Backward)
            {
                dir = MotionDirection.Idle;
            }
            currentMoveDirection = dir;
            MotionFlag speedLevel = dir == MotionDirection.Idle ? MotionFlag.Idle : data.SpeedLevel;
            AILocomotionHandler.ParamFacingMove param = new AILocomotionHandler.ParamFacingMove
            {
                Anchor = aiKnowledge.TargetKnowledge.TargetEntity,
                SpeedLevel = speedLevel,
                Duration = during,
                MovingDirection = currentMoveDirection
            };
            taskHandler.CreateFacingMoveTask(param);
        }
        private MotionDirection GetNewMoveDirection(ConfigAIFacingMoveWeight weight, FacingMoveType moveType)
        {
            float back = weight.Back;
            float forward = weight.Forward;
            float left = weight.Left;
            float right = weight.Right;
            switch (moveType)
            {
                case FacingMoveType.LeftRight:
                    forward = 0;
                    back = 0;
                    break;
                case FacingMoveType.ForwardOnly:
                    left = 0;
                    right = 0;
                    back = 0;
                    break;
                case FacingMoveType.ForwardBackward:
                    left = 0;
                    right = 0;
                    break;
            }
            
            float total = back + forward + left + right + weight.Stop;
            var value = Random.Range(0, total * 10) % 10;
            value -= back;
            if (value <= 0)
            {
                return MotionDirection.Backward;
            }
            value -= forward;
            if (value <= 0)
            {
                return MotionDirection.Forward;
            }
            value -= left;
            if (value <= 0)
            {
                return MotionDirection.Left;
            }
            value -= right;
            if (value <= 0)
            {
                return MotionDirection.Right;
            }
            return MotionDirection.Idle;
        }

        /// <summary>
        /// 检测左右靠墙
        /// </summary>
        /// <param name="aiKnowledge"></param>
        /// <param name="obstacleDetectRange"></param>
        /// <returns></returns>
        private bool CheckLRHitWall(AIKnowledge aiKnowledge, float obstacleDetectRange)
        {
            return true;
        }
    }
}