using System;

namespace TaoTie
{
    public class AIActionControlState: IDisposable
    {
        public bool IsPrepared => Status >= SkillStatus.Prepared;
        public AISkillInfo Skill;
        public SkillStatus Status;
        public long QuerySkillDiscardTick;

        public static AIActionControlState Create()
        {
            return ObjectPool.Instance.Fetch<AIActionControlState>();
        }
        
        public void Dispose()
        {
            Skill = null;
            Status = default;
            QuerySkillDiscardTick = 0;
        }

        public void Reset()
        {
            Skill = null;
            Status = SkillStatus.Inactive;
            QuerySkillDiscardTick = 0;
        }
    }
}