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
        /// <summary> 移动summary>
        public AIMoveUpdater MoveUpdater { get; private set; }= new AIMoveUpdater();
        /// <summary> 寻路 </summary>
        public AIPathfindingUpdater Pathfinder { get; private set; }= new AIPathfindingUpdater();
        /// <summary> 目标 </summary>
        public AITargetUpdater TargetUpdater { get; private set; }= new AITargetUpdater();
        /// <summary> 感知 </summary>
        public AISensingUpdater SensingUpdater { get; private set; }= new AISensingUpdater();
        /// <summary> 威胁 </summary>
        public AIThreatUpdater ThreatUpdater { get; private set; }= new AIThreatUpdater();
        /// <summary> pose </summary>
        public AIPoseControlUpdater PoseControlUpdater { get; private set; }= new AIPoseControlUpdater();
        /// <summary> 技能 </summary>
        public AISkillUpdater SkillUpdater { get; private set; }= new AISkillUpdater();

        /// <summary> 行动执行器 </summary>
        public AIActionControl ActionController { get; private set; }= new AIActionControl();
        /// <summary> 移动执行器 </summary>
        public AIMoveControl MoveController { get; private set; }= new AIMoveControl();


        #region Event

        public Action<long, long> OnThreatTargetChanged;
        public Action<ThreatLevel, ThreatLevel> OnThreatLevelChanged;
        public Action<long> OnSetCombatAttackTarget;
        #endregion
        #region IComponent

        public virtual void Init(ConfigAIBeta config)
        {
            if (SceneManager.Instance.CurrentScene is MapScene scene)
            {
                aiManager = scene.GetManager<AIManager>();
            }
            knowledge = ObjectPool.Instance.Fetch<AIKnowledge>();
            knowledge.Init(GetParent<Actor>(), config, aiManager);
            
            SensingUpdater.Init(knowledge);
            ThreatUpdater.Init(knowledge);
            TargetUpdater.Init(knowledge);
            Pathfinder.Init(knowledge);
            MoveUpdater.Init(knowledge);
            PoseControlUpdater.Init(knowledge);
            SkillUpdater.Init(knowledge);

            ActionController.Init(knowledge);
            MoveController.Init(knowledge);
            aiManager?.AddAI(this);
        }

        public virtual void Destroy()
        {
            aiManager?.RemoveAI(this);
            
            SensingUpdater.Clear();
            ThreatUpdater.Clear();
            TargetUpdater.Clear();
            Pathfinder.Clear();
            PoseControlUpdater.Clear();
            SkillUpdater.Clear();
            MoveUpdater.Clear();
            
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
            SensingUpdater.UpdateMainThread();
            ThreatUpdater.UpdateMainThread();
            TargetUpdater.UpdateMainThread();
            Pathfinder.UpdateMainThread();
            MoveUpdater.UpdateMainThread();
            
            PoseControlUpdater.UpdateMainThread();
            SkillUpdater.UpdateMainThread();
        }

        /// <summary>
        /// 决策
        /// </summary>
        private void UpdateDecision()
        {
            knowledge.TacticChanged = false;
            decisionOld.Act = decision.Act;
            decisionOld.Tactic = decision.Tactic;
            decisionOld.Move = decision.Move;
            AIDecisionTree.Think(knowledge,decision);
            if (decision.Tactic != decisionOld.Tactic)
            {
                knowledge.TacticChanged = true;
            }
            if (decision.Move != decisionOld.Move)
            {
                knowledge.MoveDecisionChanged = true;
            }
#if UNITY_EDITOR
            var transform = parent.GetComponent<GameObjectHolderComponent>()?.EntityView;
            if (transform != null)
            {
                var aiDebug = transform.GetComponent<AIDebug>();
                if (aiDebug != null)
                {
                    aiDebug.Act = decision.Act.ToString();
                    aiDebug.Tactic = decision.Tactic.ToString();
                    aiDebug.Move = decision.Move.ToString();
                    aiDebug.Target = knowledge.TargetKnowledge?.TargetEntity?.Id.ToString();
                    if (knowledge.TargetKnowledge?.TargetEntity != null)
                    {
                        aiDebug.TargetPos = knowledge.TargetKnowledge.TargetEntity.Position;
                    }
                    else
                    {
                        aiDebug.TargetPos = null;
                    }

                    aiDebug.ViewRange = knowledge.SensingKnowledge.ViewRange;
                    aiDebug.Alertness = knowledge.ThreatLevel.ToString();
                    aiDebug.SkillStatus = knowledge.ActionControlState.Status.ToString();
                }
            }
#endif
        }

        /// <summary>
        /// 执行决策结果
        /// </summary>
        private void UpdateAction()
        {
            ActionController.ExecuteAction(decision);
            MoveController.ExecuteMove(decision);
        }


        #region Public

        public void SetCombatAttackTarget(long targetRuntimeID)
        {
            OnSetCombatAttackTarget?.Invoke(targetRuntimeID);
        }
        public long GetCombatAttackTarget()
        {
            if (knowledge.TargetKnowledge?.TargetEntity != null)
            {
                return knowledge.TargetKnowledge.TargetEntity.Id;
            }
            return 0;
        }
        
        public AIDecision GetDecision() => decision;
        public AIDecision GetDecisionOld() => decisionOld;
        #endregion
    }
}