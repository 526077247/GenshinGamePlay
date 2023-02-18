using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AIThreatKnowledge: IDisposable
    {
        public ConfigAIThreatSetting config;
        /// <summary>
        /// 视觉感知威胁增加系数
        /// </summary>
        public float viewThreatGrowRate;
        /// <summary>
        /// 听觉感知威胁增加系数
        /// </summary>
        public float hearThreatGrowRate;
        /// <summary>
        /// 近身感知威胁增加系数
        /// </summary>
        public float feelThreatGrowRate;

        /// <summary>
        /// 视觉感知衰减曲线
        /// </summary>
        public AICurve viewAttenuation;
        /// <summary>
        /// 听觉感知衰减曲线
        /// </summary>
        public AICurve hearAttenuation;

        public bool reachAwareThisFrame;
        public bool reachAlertThisFrame;

        public Dictionary<int, ThreatInfo> candidateList = new();
        public Dictionary<int, ThreatInfo> threatList = new();

        /// <summary>
        /// 主要目标
        /// </summary>
        public ThreatInfo mainThreat;

        /// <summary>
        /// 威胁 广播 告知
        /// </summary>
        public float threatBroadcastRange;
        
        /// <summary>
        /// Taunt 嘲讽
        /// </summary>
        public TauntLevel resistTauntLevel;

        public static AIThreatKnowledge Create(ConfigAIBeta config)
        {
            AIThreatKnowledge res = ObjectPool.Instance.Fetch<AIThreatKnowledge>();

            res.InitSetting(config.Threat);
            return res;
        }
        
        private void InitSetting(ConfigAIThreatSetting configThreat)
        {
            config = configThreat;

            viewThreatGrowRate = configThreat.viewThreatGrow;
            hearThreatGrowRate = configThreat.hearThreatGrow;
            feelThreatGrowRate = configThreat.feelThreatGrow;

            threatBroadcastRange = configThreat.threatBroadcastRange;

            viewAttenuation = new AICurve(configThreat.viewAttenuation);
            hearAttenuation = new AICurve(configThreat.hearAttenuation);
        }

        public void ReInit()
        {
            candidateList.Clear();
            threatList.Clear();
        }

        public void Dispose()
        {
            candidateList.Clear();
            threatList.Clear();
            
            mainThreat = null;
            resistTauntLevel = default;
            
            reachAwareThisFrame = false;
            hearAttenuation = default;
            viewAttenuation = default;
            threatBroadcastRange = 0;
            feelThreatGrowRate = 0;
            hearThreatGrowRate = 0;
            viewThreatGrowRate = 0;
            config = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}