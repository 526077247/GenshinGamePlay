namespace TaoTie
{
    public class ConfigHitPattern
    {
        public string OnHitEffectName;
        public HitLevel HitLevel;
        public BaseValue HitImpulseX;
        public BaseValue HitImpulseY;
        public string HitImpulseType;
        /// <summary>
        /// 冲刺中的击退数据
        /// </summary>
        public ConfigHitImpulse OverrideHitImpulse;
        public RetreatType RetreatType;
        public float HitHaltTimeRawNum;
        public float HitHaltTimeScaleRawNum;
        public bool CanBeDefenceHalt;
        public bool MuteHitText;
        public bool Recurring;
    }
}