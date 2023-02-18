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
        public Vector3 currentPos;
        public Vector3 currentForward;
        public Vector3 eyePos;
        public Transform eyeTransform;
        
        public float temperature;
        
        public ThreatLevel threatLevel;
        public ThreatLevel threatLevelOld;
        public Vector3? enterCombatPostion;
        public Vector3? enterCombatForward;
        public DecisionArchetype decisionArchetype;
        public AITactic currentTactic;
        
        public int poseID;
        
        public AISkillKnowledge skillKnowledge;
        public AIMoveKnowledge moveKnowledge;
        public AIThreatKnowledge threatKnowledge;
        public AISensingKnowledge sensingKnowledge;
        public AIDefendAreaKnowledge defendAreaKnowledge;
        public AITargetKnowledge targetKnowledge;
        public AIPathFindingKnowledge pathFindingKnowledge;

        public void Init(Entity aiEntity, ConfigAIBeta config)
        {
            aiOwnerEntity = aiEntity as Unit;
            bornPos = aiOwnerEntity.Position;
            campID = aiOwnerEntity.CampId;
            
            sensingKnowledge = AISensingKnowledge.Create(config);
            threatKnowledge = AIThreatKnowledge.Create(config);
            targetKnowledge = AITargetKnowledge.Create();
            defendAreaKnowledge = AIDefendAreaKnowledge.Create(config,bornPos);
            skillKnowledge = AISkillKnowledge.Create(config);
            moveKnowledge = ObjectPool.Instance.Fetch<AIMoveKnowledge>();
            
            pathFindingKnowledge = ObjectPool.Instance.Fetch<AIPathFindingKnowledge>();
        }

        public void Dispose()
        {
            //注意清除引用类型
            enterCombatPostion = null;
            enterCombatForward = null;
            
            skillKnowledge.Dispose();
            skillKnowledge = null;
            moveKnowledge.Dispose();
            moveKnowledge = null;
            threatKnowledge.Dispose();
            threatKnowledge = null;
            sensingKnowledge.Dispose();
            sensingKnowledge = null;
            targetKnowledge.Dispose();
            targetKnowledge = null;
            
            ObjectPool.Instance.Recycle(this);
        }
    }
}