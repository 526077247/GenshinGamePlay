using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISensingKnowledge: IDisposable
    {
        private ConfigAISensingSetting Setting;
        private Dictionary<string, ConfigAISensingSetting> TemplateInUse;

        public Dictionary<long, SensibleInfo> EnemySensibles;
        //用于决策选取的敌人，只选最近的
        public float NearestEnemyDistance;
        public long NearestEnemy;
        /// <summary>
        /// 是否害怕中
        /// </summary>
        public bool Scared;

        public static AISensingKnowledge Create(ConfigAIBeta config)
        {
            AISensingKnowledge res = ObjectPool.Instance.Fetch<AISensingKnowledge>();
            res.Setting = config.Sensing.Setting.DeepCopy();
            res.TemplateInUse = config.Sensing.Settings;
            return res;
        }
        
        public void Dispose()
        {
            Setting = null;
            TemplateInUse = null;
            EnemySensibles = null;
            NearestEnemyDistance = 0;
            NearestEnemy = 0;
            Scared = false;
            ObjectPool.Instance.Recycle(this);
        }

        public bool EnableVision => Setting.EnableVision;
        public float ViewRange => Setting.ViewRange * Setting.Sensitivity;
        public bool ViewPanoramic => Setting.ViewPanoramic;
        public float HorizontalFov => Setting.HorizontalFov;
        public float VerticalFov => Setting.VerticalFov;
        /// <summary>水平半角。全景视觉等于没有水平限制（180°）</summary>
        public float HalfHorizontalFov => ViewPanoramic ? 180f : 0.5f * HorizontalFov;
        /// <summary>竖直半角。全景视觉等于没有竖直限制（180°）</summary>
        public float HalfVerticalFov => ViewPanoramic ? 180f : 0.5f * VerticalFov;
        public float FeelRange  => Setting.FeelRange * Setting.Sensitivity;
        public float HearAttractionRange  => Setting.HearAttractionRange * Setting.Sensitivity;
        public float HearFootstepRange => Setting.HearFootstepRange * Setting.Sensitivity;
        public float SourcelessHitAttractionRange  => Setting.SourcelessHitAttractionRange * Setting.Sensitivity;

    }
}