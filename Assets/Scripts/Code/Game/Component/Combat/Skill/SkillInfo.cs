using System.Collections.Generic;

namespace TaoTie
{
    public class SkillInfo
    {
        public uint skillID;
        // public AvatarSkillExcelConfig config;
        public float cdTimer;
        public int currChargeCount;
        public float costStamina;
        public bool canHold;
        public bool canTrigger;
        public bool useInAir;
        public HashSet<int> canUseSkillStateWhiteList;
        public MonitorType needMonitor;
        public bool isLocked;
        public bool ignoreCDMinusRatio;
        public bool forceCanDoSkill;
        public float maxHoldTime;
        public float curHoldTime;
        private float _originCDTime;
        private float _cdDelta;
        private float _cdRatio;
        private float _costElem;
        private float _costElemDelta;
        private float _costElemRatio;
        private int _maxChargeCount;
        private int _maxChargeCountDelta;
        public int skillIndex;
        public int prority;
        private List<float> _chargeTimes;
    }
}