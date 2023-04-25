namespace TaoTie
{
    public class AITimer
    {
        //开始时间
        private long startTick;
        private bool started;

        public void Start(long currentTime)
        {
            started = true;
            startTick = currentTime;
        }

        //重置开始时间
        public void Reset(long currentTime)
        {
            startTick = currentTime;
        }

        public bool IsRunning()
        {
            return started;
        }

        /// <summary>
        /// 判断是否已计时完毕
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public bool IsElapsed(long currentTime, long limit)
        {
            if (!started)
                return false;

            if(currentTime > startTick + limit)
            {
                started = false;
                return true;
            }
            return false;
        }
    }
}