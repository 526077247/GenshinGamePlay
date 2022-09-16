using System;
namespace TaoTie
{
    public class TimerAction:IDisposable
    {
        public TimerClass TimerClass;

        public object Object;

        public long Time;

        public int Type;
        public long Id;
        public static TimerAction Create(TimerClass timerClass, long time, int type, object obj)
        {
            var res = ObjectPool.Instance.Fetch(typeof (TimerAction)) as TimerAction;
            res.TimerClass = timerClass;
            res.Object = obj;
            res.Time = time;
            res.Type = type;
            res.Id = IdGenerater.Instance.GenerateId();
            return res;
        }

        public void Dispose()
        {
            this.Id = 0;
            this.Object = null;
            this.Time = 0;
            this.TimerClass = TimerClass.None;
            this.Type = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}