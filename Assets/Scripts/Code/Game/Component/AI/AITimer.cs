namespace TaoTie
{
    public class AITimer
    {
        //开始时间
        private long _startTick;
        private bool _started;

        public void Start(long currentTime)
        {
            _started = true;
            _startTick = currentTime;
        }

        //重置开始时间
        public void Reset(long currentTime)
        {
            _startTick = currentTime;
        }

        public bool IsRunning()
        {
            return _started;
        }

        /// <summary>
        /// 判断是否已计时完毕
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public bool IsElapsed(long currentTime, long limit)
        {
            if (!_started)
                return false;

            if(currentTime > _startTick + limit)
            {
                _started = false;
                return true;
            }
            return false;
        }
    }
}