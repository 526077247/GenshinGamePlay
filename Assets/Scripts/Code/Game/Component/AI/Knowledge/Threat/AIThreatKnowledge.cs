using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AIThreatKnowledge: IDisposable
    {
        public ConfigAIThreatSetting Config;
        /// <summary>
        /// 视觉感知威胁增加系数
        /// </summary>
        public float ViewThreatGrowRate;
        /// <summary>
        /// 听觉感知威胁增加系数
        /// </summary>
        public float HearThreatGrowRate;
        /// <summary>
        /// 近身感知威胁增加系数
        /// </summary>
        public float FeelThreatGrowRate;

        /// <summary>
        /// 视觉感知衰减曲线
        /// </summary>
        public AICurve ViewAttenuation;
        /// <summary>
        /// 听觉感知衰减曲线
        /// </summary>
        public AICurve HearAttenuation;

        public bool ReachAwareThisFrame;
        public bool ReachAlertThisFrame;

        private Dictionary<int, ThreatInfo> candidateList = new();
        private Dictionary<int, ThreatInfo> threatList = new();

        /// <summary>
        /// 主要目标
        /// </summary>
        public ThreatInfo MainThreat;

        /// <summary>
        /// 威胁 广播 告知
        /// </summary>
        public float ThreatBroadcastRange;
        
        /// <summary>
        /// Taunt 嘲讽
        /// </summary>
        public TauntLevel ResistTauntLevel;

        public static AIThreatKnowledge Create(ConfigAIBeta config)
        {
            AIThreatKnowledge res = ObjectPool.Instance.Fetch<AIThreatKnowledge>();

            res.InitSetting(config.Threat);
            return res;
        }
        
        private void InitSetting(ConfigAIThreatSetting configThreat)
        {
            Config = configThreat;

            ViewThreatGrowRate = configThreat.ViewThreatGrow;
            HearThreatGrowRate = configThreat.HearThreatGrow;
            FeelThreatGrowRate = configThreat.FeelThreatGrow;

            ThreatBroadcastRange = configThreat.ThreatBroadcastRange;

            ViewAttenuation = new AICurve(configThreat.ViewAttenuation);
            HearAttenuation = new AICurve(configThreat.HearAttenuation);
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
            
            MainThreat = null;
            ResistTauntLevel = default;
            
            ReachAwareThisFrame = false;
            HearAttenuation = default;
            ViewAttenuation = default;
            ThreatBroadcastRange = 0;
            FeelThreatGrowRate = 0;
            HearThreatGrowRate = 0;
            ViewThreatGrowRate = 0;
            Config = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}