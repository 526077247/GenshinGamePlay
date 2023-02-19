using UnityEngine;

namespace TaoTie
{
    public class ThreatInfo
    {
        public const float TEMPEVAL_MIN = 0f;
        public const float TEMPERVAL_MAX = 100f;
        public const float TEMPERVAL_CLERAR = 0f;
        public const float TEMPERVAL_AWARE = 10f;
        public const float TEMPERVAL_ALERT = 100f;

        public const float THREATVAL_MIN = 0F;
        public const float THREATVAL_MAX = 99999f;
        public const float THREATVAL_HIT_ONCE = 20f;
        public const float THREATVAL_MAINTHREATE_DROP = 0.03f;
        public const float THREATVAL_THREATLOST = 1f;
        //TODO
        //public const float TIME_HOLDTHREAT_AVATARCHANGED = 2f;

        public long id;
        public ThreatAddReason addReason;
        //警戒值
        public float temperature = 0f;
        //威胁值
        public float threatValue = 0f;

        public Vector3 threatPos;

        public float lastSeenTime;
        public float lastFeelTime;
        public float lastFootstepTime;

        public float caredGlobalValue;

        public AITimer lctByFarDistance = new();
        public AITimer lctByEntityDisappear = new();
        public AITimer lctByOutOfZone = new();

        public ThreatInfo(long id, Vector3 position, ThreatAddReason reason)
        {
            this.id = id;
            this.threatPos = position;
            this.addReason = reason;
        }

        public void DecreaseTemper(float amount)
        {
            temperature -= amount;
            if (temperature < TEMPEVAL_MIN)
                temperature = TEMPEVAL_MIN;
        }

        public void IncreaseTemper(float amount)
        {
            temperature += amount;
            if (temperature > TEMPERVAL_MAX)
                temperature = TEMPERVAL_MAX;
        }

        public void IncreaseThreat(float amount)
        {
            threatValue += amount;
            if (threatValue > THREATVAL_MAX)
                threatValue = THREATVAL_MAX;
        }
        public void DecreaseThreat(float amount)
        {
            threatValue -= amount;
            if (threatValue < THREATVAL_MIN)
                threatValue = THREATVAL_MIN;
        }

        //TODO 不知道做什么
        public void Hold(float currentTime)
        {
            float lastSensibleTime = lastSeenTime > lastFeelTime ? lastSeenTime : lastFeelTime;
            lastSeenTime = lastSensibleTime > lastFootstepTime ? lastSensibleTime : lastFootstepTime;
        }
    }
}