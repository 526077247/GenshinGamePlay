using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary> 巡逻跟随：沿预设路点循环巡逻 </summary>
    public class PatrolFollowInfo : MoveInfoBase
    {
        public enum Status
        {
            Inactive = 0,
            Patrolling = 1
        }

        public Status status;
        private int currentIndex;
        private Vector3[] patrolPoints;

        public static PatrolFollowInfo Create()
        {
            return ObjectPool.Instance.Fetch<PatrolFollowInfo>();
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                patrolPoints = GenerateDefaultPatrol(aiKnowledge);
                currentIndex = 0;
            }
            StartPatrol(taskHandler, aiKnowledge);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (status != Status.Patrolling) return;
            if (taskHandler.CurrentState == LocoTaskState.Finished)
            {
                currentIndex = (currentIndex + 1) % patrolPoints.Length;
                StartPatrol(taskHandler, aiKnowledge);
            }
        }

        private void StartPatrol(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                status = Status.Inactive;
                return;
            }
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = patrolPoints[currentIndex],
                SpeedLevel = MotionFlag.Walk
            };
            taskHandler.CreateGoToTask(param);
            status = Status.Patrolling;
        }

        private Vector3[] GenerateDefaultPatrol(AIKnowledge aiKnowledge)
        {
            Vector3 born = aiKnowledge.BornPos;
            return new Vector3[]
            {
                born + new Vector3(3, 0, 0),
                born + new Vector3(0, 0, 3),
                born + new Vector3(-3, 0, 0),
                born + new Vector3(0, 0, -3)
            };
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            status = Status.Inactive;
        }

        public override void Dispose()
        {
            status = default;
            currentIndex = 0;
            patrolPoints = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}
