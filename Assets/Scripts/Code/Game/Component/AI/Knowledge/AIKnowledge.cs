using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 搜集的数据
    /// </summary>
    public class AIKnowledge: IDisposable
    {
        public Actor AiOwnerEntity; // 0x18
        public uint CampID;
        
        public Vector3 BornPos;
        public Vector3 CurrentPos => AiOwnerEntity.Position;
        public Vector3 CurrentForward => AiOwnerEntity.Forward;
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
        
        public AIManager AiManager;
        public CombatComponent CombatComponent => AiOwnerEntity.GetComponent<CombatComponent>();
        public PoseFSMComponent Pose => AiOwnerEntity.GetComponent<PoseFSMComponent>();
        
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
        public void Init(Actor aiEntity, ConfigAIBeta config, AIManager aiManager)
        {
            this.AiManager = aiManager;
            AiOwnerEntity = aiEntity;
            BornPos = AiOwnerEntity.Position;
            CampID = AiOwnerEntity.CampId;
            DecisionArchetype = config.DecisionArchetype;
            
            MoveControlState = AIMoveControlState.Create();
            ActionControlState = AIActionControlState.Create();
            
            SensingKnowledge = AISensingKnowledge.Create(config);
            ThreatKnowledge = AIThreatKnowledge.Create(config);
            TargetKnowledge = AITargetKnowledge.Create();
            DefendAreaKnowledge = AIDefendAreaKnowledge.Create(config,BornPos);
            SkillKnowledge = AISkillKnowledge.Create(config);
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
            
            AiManager = null;
            AiOwnerEntity = null;
            BornPos = default;
            CampID = 0;
            EnterCombatPosition = null;
            EnterCombatForward = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}