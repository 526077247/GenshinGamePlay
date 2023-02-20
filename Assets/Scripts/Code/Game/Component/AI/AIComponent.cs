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
        protected AIDecision decision { get; private set; }= new AIDecision();
        /// <summary> 上一帧决策结果 </summary>
        protected AIDecision decisionOld { get; private set; }= new AIDecision();

        /// <summary> 寻路 </summary>
        public AIPathfindingUpdater pathfinder { get; private set; }= new AIPathfindingUpdater();
        /// <summary> 目标 </summary>
        public AITargetUpdater targetUpdater { get; private set; }= new AITargetUpdater();
        /// <summary> 感知 </summary>
        public AISensingUpdater sensingUpdater { get; private set; }= new AISensingUpdater();
        /// <summary> 威胁 </summary>
        public AIThreatUpdater threatUpdater { get; private set; }= new AIThreatUpdater();
        /// <summary> pose </summary>
        public AIPoseControlUpdater poseControlUpdater { get; private set; }= new AIPoseControlUpdater();
        /// <summary> 技能 </summary>
        public AISkillUpdater skillUpdater { get; private set; }= new AISkillUpdater();

        /// <summary> 行动执行器 </summary>
        public AIActionControl actionController { get; private set; }= new AIActionControl();
        /// <summary> 移动执行器 </summary>
        public AIMoveControl moveController { get; private set; }= new AIMoveControl();


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
            knowledge.Init(GetParent<Unit>(), config, aiManager);
            
            sensingUpdater.Init(knowledge);
            threatUpdater.Init(knowledge);
            targetUpdater.Init(knowledge);
            pathfinder.Init(knowledge);
            
            poseControlUpdater.Init(knowledge);
            skillUpdater.Init(knowledge);

            actionController.Init(knowledge);
            moveController.Init(knowledge);
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