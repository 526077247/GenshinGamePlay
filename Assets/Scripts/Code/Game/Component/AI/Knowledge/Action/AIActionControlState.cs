using System;

namespace TaoTie
{
    public class AIActionControlState: IDisposable
    {
        public bool isPrepared;
        public AISkillInfo skill;
        public SkillStatus status;
        public float querySkillDiscardTick;
        public string currentStateID;

        public static AIActionControlState Create()
        {
            return ObjectPool.Instance.Fetch<AIActionControlState>();
        }
        
        public void Dispose()
        {
            isPrepared = false;
            skill = null;
            status = default;
            querySkillDiscardTick = 0;
            currentStateID = null;
        }

        public void Reset()
        {
            skill = null;
            currentStateID = null;
            status = SkillStatus.Inactive;
        }
    }
}