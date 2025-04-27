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

            ActionControlState.Dispose();
            ActionControlState = null;
            MoveControlState.Dispose();
            MoveControlState = null;

            ThreatLevel = default;
            ThreatLevelOld = default;
            DecisionArchetype = default;
            CurrentTactic = default;
            TacticChanged = false;
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