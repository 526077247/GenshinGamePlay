using System;

namespace TaoTie
{
    public class AIActionControlState: IDisposable
    {
        private bool isPrepared;
        public AISkillInfo Skill;
        public SkillStatus Status;
        private float QuerySkillDiscardTick;

        public static AIActionControlState Create()
        {
            return ObjectPool.Instance.Fetch<AIActionControlState>();
        }
        
        public void Dispose()
        {
            isPrepared = false;
            Skill = null;
            Status = default;
            QuerySkillDiscardTick = 0;
        }

        public void Reset()
        {
            Skill = null;
            Status = SkillStatus.Inactive;
        }
    }
}