using System;
using UnityEngine;

namespace TaoTie
{
    public class ThreatInfo: IDisposable
    {
        public const float TEMPEVAL_MIN = 0f;
        public const float TEMPERVAL_MAX = 100f;
        public const float TEMPERVAL_CLERAR = 0f;
        public const float TEMPERVAL_AWARE = 10f;
        public const float TEMPERVAL_ALERT = 100f;

        public const float THREATVAL_MIN = 0F;
        public const float THREATVAL_MAX = 99999f;
        public const float THREATVAL_MAINTHREATE_DROP = 0.03f;
        public const float THREATVAL_THREATLOST = 1f;
        public const float TIME_HOLDTHREAT_AVATARCHANGED = 2f;

        public long Id;
        public ThreatAddReason AddReason;
        //警戒值
        public float Temperature { get; private set; }= 0f;
        //威胁值
        public float ThreatValue { get; private set; }= 0f;

        public Vector3 ThreatPos;

        public float LastSeenTime;
        public float LastFeelTime;
        public float LastFootstepTime;

        public float CaredGlobalValue;

        public AITimer LctByFarDistance;
        public AITimer LctByEntityDisappear;
        public AITimer LctByOutOfZone;

        public static ThreatInfo Create(long id, Vector3 position, ThreatAddReason reason)
        {
            ThreatInfo res = ObjectPool.Instance.Fetch<ThreatInfo>();
            res.Init(id, position, reason);
            return res;
        }
        private void Init(long id, Vector3 position, ThreatAddReason reason)
        {
            this.Id = id;
            this.ThreatPos = position;
            this.AddReason = reason;
            LctByFarDistance = AITimer.Create();
            LctByEntityDisappear = AITimer.Create();
            LctByOutOfZone = AITimer.Create();
        }

        public void DecreaseTemper(float amount)
        {
            Temperature -= amount;
            if (Temperature < TEMPEVAL_MIN)
                Temperature = TEMPEVAL_MIN;
        }

        public void IncreaseTemper(float amount)
        {
            Temperature += amount;
            if (Temperature > TEMPERVAL_MAX)
                Temperature = TEMPERVAL_MAX;
        }

        public void IncreaseThreat(float amount)
        {
            ThreatValue += amount;
            if (ThreatValue > THREATVAL_MAX)
                ThreatValue = THREATVAL_MAX;
        }
        public void DecreaseThreat(float amount)
        {
            ThreatValue -= amount;
            if (ThreatValue < THREATVAL_MIN)
                ThreatValue = THREATVAL_MIN;
        }

        public void Dispose()
        {
            this.Id = 0;
            this.ThreatPos = default;
            this.AddReason = default;
            LctByFarDistance?.Dispose();
            LctByEntityDisappear?.Dispose();
            LctByOutOfZone?.Dispose();
            LctByFarDistance = null;
            LctByEntityDisappear = null;
            LctByOutOfZone = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}