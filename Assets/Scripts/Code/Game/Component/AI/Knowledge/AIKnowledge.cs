using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 搜集的数据
    /// </summary>
    public class AIKnowledge: IDisposable
    {
        public Actor Entity;
        public uint CampID;
        
        public Vector3 BornPos;
        public Vector3 CurrentPos => Entity.Position;
        public Vector3 CurrentForward => Entity.Forward;
        public Vector3 EyePos;
        public Transform EyeTransform;
        
        public float Temperature;
        
        public ThreatLevel ThreatLevel;
        public ThreatLevel ThreatLevelOld;
        public Vector3? EnterCombatPosition;
        public Vector3? EnterCombatForward;
        public DecisionArchetype DecisionArchetype;
        public AITactic CurrentTactic;
        public bool TacticChanged;
        public bool MoveDecisionChanged;
        public AIMoveControlState MoveControlState;
        public AIActionControlState ActionControlState;
        
        public int PoseID;
        
        public AIManager AIManager;
        public CombatComponent CombatComponent => Entity.GetComponent<CombatComponent>();
        public PoseFSMComponent Pose => Entity.GetComponent<PoseFSMComponent>();
        public MoveComponent Mover => Entity.GetComponent<MoveComponent>();
        public AIInputController Input => Entity.GetComponent<AIInputController>();
        
        public ORCAAgentComponent OrcaAgent => Entity.GetComponent<ORCAAgentComponent>();
        public NumericComponent Numeric => Entity.GetComponent<NumericComponent>();
        public SkillComponent SkillComponent => Entity.GetComponent<SkillComponent>();
        
        public AISkillKnowledge SkillKnowledge;
        public AIMoveKnowledge MoveKnowledge;
        public AIThreatKnowledge ThreatKnowledge;
        public AISensingKnowledge SensingKnowledge;
        public AIDefendAreaKnowledge DefendAreaKnowledge;
        public AITargetKnowledge TargetKnowledge;
        public AIPathFindingKnowledge PathFindingKnowledge;
        
        
        public AITacticKnowledge_FacingMove FacingMoveTactic;
        public AITacticKnowledge_MeleeCharge MeleeChargeTactic;
        public AITacticKnowledge_Flee FleeTactic;
        public AITacticKnowledge_Wander WanderTactic;

        /// <summary>
        /// 此刻能不能「看见」目标：视觉开启 + 视野距离内 + 视锥内 + 视线无遮挡。
        ///
        /// 为什么不直接查 <see cref="AISensingKnowledge.EnemySensibles"/>：
        /// 1. 那是上一次感知帧的快照，而受击结算可能发生在感知之前（同一帧内顺序不确定），
        ///    用快照会让「刚转身看到射手」这一发变成无源伤害；
        /// 2. 感知列表里 FeelRange 内的目标不需要视线也不需要朝向（贴脸感知），
        ///    拿它当「看见」会把背后贴着的人也算成可见；
        /// 3. 部分 AI 根本没开视觉（EnableVision=false），此时列表里照样有目标。
        ///
        /// 视锥与视线判定和感知筛共用 <see cref="AISensingUpdater"/> 的静态方法，
        /// 保证「看得见」和「感知得到」不会出现两套口径。
        /// </summary>
        public bool CanSee(Actor target)
        {
            if (target == null || Entity == null) return false;
            var sensing = SensingKnowledge;
            if (sensing == null || !sensing.EnableVision) return false;

            var selfPos = Entity.Position;
            var targetPos = target.Position;
            var delta = targetPos - selfPos;
            var distance = delta.magnitude;
            if (distance > sensing.ViewRange) return false;

            var forward = EyeTransform != null ? EyeTransform.forward : Entity.Forward;
            var direction = distance > 1e-4f ? delta / distance : forward;
            if (!AISensingUpdater.IsInViewCone(forward, direction, sensing.HalfHorizontalFov, sensing.HalfVerticalFov))
                return false;

            var eyePos = EyePos;
            if (eyePos == Vector3.zero)
            {
                // EyePos 由 AISensingUpdater 每帧写入；没开感知模块的 AI 永远是零向量，
                // 那样视线会从世界原点打出去，判定完全失真，所以这里按模型高度兜一个眼部位置。
                eyePos = selfPos + Vector3.up * (Entity.ConfigActor?.Common?.ModelHeight ?? 0f);
            }

            var height = target.ConfigActor?.Common?.ModelHeight ?? 0f;
            return AISensingUpdater.CheckLineOfSight(eyePos, targetPos, targetPos + Vector3.up * height, true);
        }

        public void Init(Actor aiEntity, ConfigAIBeta config, AIManager aiManager, ConfigShape defendArea)
        {
            AIManager = aiManager;
            Entity = aiEntity;
            BornPos = Entity.Position;
            CampID = Entity.CampId;
            DecisionArchetype = config.DecisionArchetype;

            SkillKnowledge = AISkillKnowledge.Create(config);
            MoveControlState = AIMoveControlState.Create();
            ActionControlState = AIActionControlState.Create();

            SensingKnowledge = AISensingKnowledge.Create(config);
            ThreatKnowledge = AIThreatKnowledge.Create(config);
            TargetKnowledge = AITargetKnowledge.Create();
            DefendAreaKnowledge = AIDefendAreaKnowledge.Create(config, BornPos, defendArea);
            MoveKnowledge = AIMoveKnowledge.Create(config);

            PathFindingKnowledge = AIPathFindingKnowledge.Create(config);

            FacingMoveTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_FacingMove>();
            FacingMoveTactic.LoadData(config.FacingMoveTactic);
            MeleeChargeTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_MeleeCharge>();
            MeleeChargeTactic.LoadData(config.MeleeChargeTactic);
            FleeTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_Flee>();
            FleeTactic.LoadData(config.FleeTactic);
            WanderTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_Wander>();
            WanderTactic.LoadData(config.WanderTactic);
        }

        public void Dispose()
        {
            //注意清除引用类型
            EnterCombatPosition = null;
            EnterCombatForward = null;
            
            WanderTactic.Dispose();
            WanderTactic = null;
            FleeTactic.Dispose();
            FleeTactic = null;
            MeleeChargeTactic.Dispose();
            MeleeChargeTactic = null;
            FacingMoveTactic.Dispose();
            FacingMoveTactic = null;
            
            PathFindingKnowledge.Dispose();
            PathFindingKnowledge = null;
            
            MoveKnowledge.Dispose();
            MoveKnowledge = null;
            SkillKnowledge.Dispose();
            SkillKnowledge = null;
            ThreatKnowledge.Dispose();
            ThreatKnowledge = null;
            SensingKnowledge.Dispose();
            SensingKnowledge = null;
            TargetKnowledge.Dispose();
            TargetKnowledge = null;
            DefendAreaKnowledge.Dispose();
            DefendAreaKnowledge = null;

            ActionControlState.Dispose();
            ActionControlState = null;
            MoveControlState.Dispose();
            MoveControlState = null;

            ThreatLevel = default;
            ThreatLevelOld = default;
            DecisionArchetype = default;
            CurrentTactic = default;
            TacticChanged = false;
            MoveDecisionChanged = false;
            Temperature = 0f;
            EyePos = default;
            EyeTransform = null;
            PoseID = 0;
            AIManager = null;
            Entity = null;
            BornPos = default;
            CampID = 0;
            EnterCombatPosition = null;
            EnterCombatForward = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}