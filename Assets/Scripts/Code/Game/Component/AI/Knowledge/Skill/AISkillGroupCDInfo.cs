using System;

namespace TaoTie
{
    public class AISkillGroupCDInfo : IDisposable
    {
        public int GroupID;
        public int GroupCDTime;
        public long NextCDTick;

        public static AISkillGroupCDInfo Create(int id,int cd)
        {
            AISkillGroupCDInfo res = ObjectPool.Instance.Fetch<AISkillGroupCDInfo>();
            res.GroupID = id;
            res.GroupCDTime = cd;
            res.NextCDTick = 0;
            return res;
        }

        public void Dispose()
        {
            GroupID = default;
            GroupCDTime = default;
            NextCDTick = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}