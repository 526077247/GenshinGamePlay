using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AIThreatKnowledge: IDisposable
    {
        public ConfigAIThreatSetting config;
        //视觉
        public float viewThreatGrowRate;
        //听觉
        public float hearThreatGrowRate;
        //近身感知
        public float feelThreatGrowRate;

        public AIMath.AICurve viewAttenuation;
        public AIMath.AICurve hearAttenuation;

        public bool reachAwareThisFrame;
        public bool reachAlertThisFrame;

        public Dictionary<int, ThreatInfo> candidateList = new();
        public Dictionary<int, ThreatInfo> threatList = new();

        //主要目标
        public ThreatInfo mainThreat;

        //广播 告知
        public float threatBroadcastRange;

        //TODO
        //Taunt 嘲讽

        public void InitSetting(ConfigAIThreatSetting configThreat)
        {
            config = configThreat;

            viewThreatGrowRate = configThreat.viewThreatGrow;
            hearThreatGrowRate = configThreat.hearThreatGrow;
            feelThreatGrowRate = configThreat.feelThreatGrow;

            threatBroadcastRange = configThreat.threatBroadcastRange;

            viewAttenuation = new AIMath.AICurve(configThreat.viewAttenuation);
            hearAttenuation = new AIMath.AICurve(configThreat.hearAttenuation);
        }

        public void ReInit()
        {
            candidateList.Clear();
            threatList.Clear();
        }

        public void Dispose()
        {
            
        }
    }
}