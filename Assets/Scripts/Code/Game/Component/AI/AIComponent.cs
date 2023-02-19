using System;
using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// AI组件，注意回收的问题
    /// </summary>
    public class AIComponent: Component,IComponent<ConfigAIBeta>
    {

        protected AIManager aiManager;
        /// <summary> 收集的信息 </summary>
        protected AIKnowledge knowledge;

        /// <summary> 这一帧决策结果 </summary>
        protected AIDecision decision => new AIDecision();
        /// <summary> 上一帧决策结果 </summary>
        protected AIDecision decisionOld => new AIDecision();

        /// <summary> 寻路 </summary>
        protected AIPathfindingUpdater pathfinder => new AIPathfindingUpdater();
        /// <summary> 目标 </summary>
        protected AITargetUpdater targetUpdater => new AITargetUpdater(this);
        /// <summary> 感知 </summary>
        protected AISensingUpdater sensingUpdater => new AISensingUpdater(aiManager);
        /// <summary> 威胁 </summary>
        protected AIThreatUpdater threatUpdater => new AIThreatUpdater(this);
        /// <summary> pose </summary>
        protected AIPoseControlUpdater poseControlUpdater => new AIPoseControlUpdater();
        /// <summary> 技能 </summary>
        protected AISkillUpdater skillUpdater => new AISkillUpdater();

        /// <summary> 行动执行器 </summary>
        protected AIActionControl actionController => new AIActionControl(knowledge,this);
        /// <summary> 移动执行器 </summary>
        protected AIMoveControl moveController => new AIMoveControl(this,knowledge,pathfinder);


        #region Event

        public Action<long, long> OnThreatTargetChanged;
        public Action<ThreatLevel, ThreatLevel> OnThreatLevelChanged;
        public Action<long> OnSetCombatAttackTarget;
        #endregion
        #region IComponent

        public virtual void Init(ConfigAIBeta config)
        {
            if (SceneManager.Instance.CurrentScene is BaseMapScene scene)
            {
                aiManager = scene.GetManager<AIManager>();
            }
            knowledge = ObjectPool.Instance.Fetch<AIKnowledge>();
            knowledge.Init(Parent, config, aiManager);
            
            sensingUpdater.Init(knowledge);
            threatUpdater.Init(knowledge);
            targetUpdater.Init(knowledge);
            pathfinder.Init(knowledge);
            
            poseControlUpdater.Init(knowledge);
            skillUpdater.Init(knowledge);

            aiManager?.AddAI(this);
        }

        public virtual void Destroy()
        {
            aiManager?.RemoveAI(this);
            
            sensingUpdater.Clear();
            threatUpdater.Clear();
            targetUpdater.Clear();
            pathfinder.Clear();
            poseControlUpdater.Clear();
            skillUpdater.Clear();
            
            knowledge.Dispose();
            knowledge = null;
            aiManager = null;
        }

        public virtual void Update()
        {
            //更新知识
            UpdateKnowledge();
            
            //决策
            UpdateDecision();
            
            //执行
            UpdateAction();
        }

        #endregion

        /// <summary>
        /// 主线程更新知识
        /// </summary>
        private void UpdateKnowledge()
        {
            sensingUpdater.UpdateMainThread();
            threatUpdater.UpdateMainThread();
            targetUpdater.UpdateMainThread();
            pathfinder.UpdateMainThread();
            
            poseControlUpdater.UpdateMainThread();
            skillUpdater.UpdateMainThread();
        }

        /// <summary>
        /// 决策
        /// </summary>
        private void UpdateDecision()
        {
            decisionOld.act = decision.act;
            decisionOld.tactic = decision.tactic;
            decisionOld.move = decision.move;
            AIDecisionTree.Think(knowledge,decision);
        }

        /// <summary>
        /// 执行决策结果
        /// </summary>
        private void UpdateAction()
        {
            actionController.ExecuteAction(decision);
            moveController.ExecuteMove(decision);
        }


        #region Public

        public void SetCombatAttackTarget(long targetRuntimeID)
        {
            OnSetCombatAttackTarget?.Invoke(targetRuntimeID);
        }
        public long GetCombatAttackTarget()
        {
            return 0;
        }
        #endregion
    }
}