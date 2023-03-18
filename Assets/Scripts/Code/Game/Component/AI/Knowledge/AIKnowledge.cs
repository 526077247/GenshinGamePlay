using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 搜集的数据
    /// </summary>
    public class AIKnowledge: IDisposable
    {
        public Unit aiOwnerEntity; // 0x18
        public uint campID;
        
        public Vector3 bornPos;
        public Vector3 currentPos => aiOwnerEntity.Position;
        public Vector3 currentForward => aiOwnerEntity.Forward;
        public Vector3 eyePos;
        public Transform eyeTransform;
        
        public float temperature;
        
        public ThreatLevel threatLevel;
        public ThreatLevel threatLevelOld;
        public Vector3? enterCombatPosition;
        public Vector3? enterCombatForward;
        public DecisionArchetype decisionArchetype;
        public AITactic currentTactic;
        public bool tacticChanged;
        public bool moveDecisionChanged;
        public AIMoveControlState moveControlState;
        public AIActionControlState actionControlState;
        
        public int poseID;
        
        public AIManager aiManager;
        public CombatComponent combatComponent => aiOwnerEntity.GetComponent<CombatComponent>();
        public PoseFSMComponent pose => aiOwnerEntity.GetComponent<PoseFSMComponent>();
        
        public AISkillKnowledge skillKnowledge;
        public AIMoveKnowledge moveKnowledge;
        public AIThreatKnowledge threatKnowledge;
        public AISensingKnowledge sensingKnowledge;
        public AIDefendAreaKnowledge defendAreaKnowledge;
        public AITargetKnowledge targetKnowledge;
        public AIPathFindingKnowledge pathFindingKnowledge;
        
        
        public AITacticKnowledge_FacingMove facingMoveTactic;
        public AITacticKnowledge_MeleeCharge meleeChargeTactic;
        public AITacticKnowledge_Flee fleeTactic;
        public AITacticKnowledge_Wander wanderTactic;
        public void Init(Unit aiEntity, ConfigAIBeta config, AIManager aiManager)
        {
            this.aiManager = aiManager;
            aiOwnerEntity = aiEntity;
            bornPos = aiOwnerEntity.Position;
            campID = aiOwnerEntity.CampId;
            decisionArchetype = config.DecisionArchetype;
            
            moveControlState = AIMoveControlState.Create();
            actionControlState = AIActionControlState.Create();
            
            sensingKnowledge = AISensingKnowledge.Create(config);
            threatKnowledge = AIThreatKnowledge.Create(config);
            targetKnowledge = AITargetKnowledge.Create();
            defendAreaKnowledge = AIDefendAreaKnowledge.Create(config,bornPos);
            skillKnowledge = AISkillKnowledge.Create(config);
            moveKnowledge = AIMoveKnowledge.Create(config);
            
            pathFindingKnowledge = AIPathFindingKnowledge.Create(config);
            
            facingMoveTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_FacingMove>();
            facingMoveTactic.LoadData(config.FacingMoveTactic);
            meleeChargeTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_MeleeCharge>();
            meleeChargeTactic.LoadData(config.MeleeChargeTactic);
            fleeTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_Flee>();
            fleeTactic.LoadData(config.FleeTactic);
            wanderTactic = ObjectPool.Instance.Fetch<AITacticKnowledge_Wander>();
            wanderTactic.LoadData(config.WanderTactic);
        }

        public void Dispose()
        {
            //注意清除引用类型
            enterCombatPosition = null;
            enterCombatForward = null;
            
            wanderTactic.Dispose();
            wanderTactic = null;
            fleeTactic.Dispose();
            fleeTactic = null;
            meleeChargeTactic.Dispose();
            meleeChargeTactic = null;
            facingMoveTactic.Dispose();
            facingMoveTactic = null;
            
            pathFindingKnowledge.Dispose();
            pathFindingKnowledge = null;
            
            moveKnowledge.Dispose();
            moveKnowledge = null;
            skillKnowledge.Dispose();
            skillKnowledge = null;
            threatKnowledge.Dispose();
            threatKnowledge = null;
            sensingKnowledge.Dispose();
            sensingKnowledge = null;
            targetKnowledge.Dispose();
            targetKnowledge = null;

            actionControlState.Dispose();
            actionControlState = null;
            moveControlState.Dispose();
            moveControlState = null;
            
            aiManager = null;
            aiOwnerEntity = null;
            bornPos = default;
            campID = 0;
            enterCombatPosition = null;
            enterCombatForward = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}