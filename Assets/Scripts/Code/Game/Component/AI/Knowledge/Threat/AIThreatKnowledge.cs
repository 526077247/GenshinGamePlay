using System;
using System.Collections.Generic;
using UnityEngine;

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
        public AnimationCurve ViewAttenuation;
        /// <summary>
        /// 听觉感知衰减曲线
        /// </summary>
        public AnimationCurve HearAttenuation;

        public bool ReachAwareThisFrame;
        public bool ReachAlertThisFrame;

        private Dictionary<int, ThreatInfo> candidateList = new Dictionary<int, ThreatInfo>();
        private Dictionary<int, ThreatInfo> threatList = new Dictionary<int, ThreatInfo>();

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

            ViewAttenuation = configThreat.ViewAttenuationCurve;
            HearAttenuation = configThreat.HearAttenuationCurve;
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