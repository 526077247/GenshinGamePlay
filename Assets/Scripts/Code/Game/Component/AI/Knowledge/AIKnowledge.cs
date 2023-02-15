using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 搜集的数据
    /// </summary>
    public class AIKnowledge: IDisposable
    {
        public Vector3 bornPos;
        public Vector3 currentPos;
        public Vector3 currentForward;
        
        public ThreatLevel threatLevel;
        public ThreatLevel threatLevelOld;
        public Vector3? enterCombatPostion;
        public Vector3? enterCombatForward;
        public DecisionArchetype decisionArchetype;
        public AITactic currentTactic;
        
        public int poseID;

        public AISensingKnowledge snsingKnowledge;
        public AISkillKnowledge skillKnowledge;
        public AIMoveKnowledge moveKnowledge;
        public AIThreatKnowledge threatKnowledge;

        public void Init(Entity aiEntity, ConfigAIBeta config)
        {
            
        }

        public void Dispose()
        {
            //注意清除引用类型
            enterCombatPostion = null;
            enterCombatForward = null;
            
            snsingKnowledge.Dispose();
            snsingKnowledge = null;
            skillKnowledge.Dispose();
            skillKnowledge = null;
            moveKnowledge.Dispose();
            moveKnowledge = null;
            threatKnowledge.Dispose();
            threatKnowledge = null;
            
            ObjectPool.Instance.Recycle(this);
        }
    }
}