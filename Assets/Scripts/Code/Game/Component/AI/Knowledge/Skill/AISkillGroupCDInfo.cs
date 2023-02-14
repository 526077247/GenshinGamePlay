using System;

namespace TaoTie
{
    public class AISkillGroupCDInfo : IDisposable
    {
        public int groupID;
        public int groupCDTime;
        public long nextCDTick;

        public void Dispose()
        {
            groupID = default;
            groupCDTime = default;
            nextCDTick = default;
        }
    }
}