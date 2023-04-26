using System.Collections.Generic;

namespace TaoTie
{
    public class SkillInfo
    {
        public uint SkillID;
        // public AvatarSkillExcelConfig config;
        public float CdTimer;
        public int CurrChargeCount;
        public float CostStamina;
        public bool CanHold;
        public bool CanTrigger;
        public bool UseInAir;
        public HashSet<int> CanUseSkillStateWhiteList;
        public MonitorType NeedMonitor;
        public bool IsLocked;
        public bool InoreCDMinusRatio;
        public bool ForceCanDoSkill;
        public float MaxHoldTime;
        public float CurHoldTime;
        private float orginCDTime;
        private float cdDelta;
        private float cdRatio;
        private float costElem;
        private float costElemDelta;
        private float costElemRatio;
        private int maxChargeCount;
        private int maxChargeCountDelta;
        public int SkillIndex;
        public int Prority;
        private List<float> chargeTimes;
    }
}