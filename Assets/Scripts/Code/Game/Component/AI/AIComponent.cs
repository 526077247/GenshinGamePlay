using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// AI组件，注意回收的问题
    /// </summary>
    public class AIComponent: Component,IComponent<ConfigAIBeta>
    {

        /// <summary> 收集的信息 </summary>
        protected AIKnowledge knowledge;

        /// <summary> 这一帧决策结果 </summary>
        protected AIDecision decision => new AIDecision();
        /// <summary> 上一帧决策结果 </summary>
        protected AIDecision decisionOld => new AIDecision();

        /// <summary> 寻路 </summary>
        protected AIPathfinding pathfinder => new AIPathfinding();

        /// <summary> 感知 </summary>
        protected AISensingUpdater sensingUpdater => new AISensingUpdater(this);
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
        
        #region IComponent

        public virtual void Init(ConfigAIBeta config)
        {
            knowledge = ObjectPool.Instance.Fetch<AIKnowledge>();
            knowledge.Init(Parent,config);
            
            sensingUpdater.Init(knowledge);
            pathfinder.Init(knowledge);
            threatUpdater.Init(knowledge);
            poseControlUpdater.Init(knowledge);
            skillUpdater.Init(knowledge);

            if (SceneManager.Instance.CurrentScene is BaseMapScene scene)
            {
                scene.GetManager<AIManager>().AddAI(this);
            }
        }

        public virtual void Destroy()
        {
            if (SceneManager.Instance.CurrentScene is BaseMapScene scene)
            {
                scene.GetManager<AIManager>().RemoveAI(this);
            }
            
            pathfinder.Clear();
            threatUpdater.Clear();
            poseControlUpdater.Clear();
            skillUpdater.Clear();
            
            knowledge.Dispose();
            knowledge = null;
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
            pathfinder.UpdateMainThread();
            sensingUpdater.UpdateMainThread();
            threatUpdater.UpdateMainThread();
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

    }
}