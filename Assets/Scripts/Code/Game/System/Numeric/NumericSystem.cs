using System.Collections.Generic;

namespace TaoTie
{
    public class NumericSystem:IManager
    {
        public List<NumericComponent> Data;
        private long Timer;

        private long lastUpdateTime;
        #region override
        
        [Timer(TimerType.NumericUpdate)]
        public class NumericUpdateTimer:ATimer<NumericSystem>
        {
            public override void Run(NumericSystem t)
            {
                t.Update();
            }
        }

        public void Init()
        {
            Data = new List<NumericComponent>();
            Timer = GameTimerManager.Instance.NewRepeatedTimer(250, TimerType.NumericUpdate, this);
            lastUpdateTime = GameTimerManager.Instance.GetTimeNow();
        }

        public void Destroy()
        {
            GameTimerManager.Instance.Remove(ref Timer);
            Data.Clear();
            Data = null;
        }

        public void Update()
        {
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            long deltaTime = timeNow - lastUpdateTime;
            //遍历回血，回蓝
            for (int i = 0; i < Data.Count; i++)
            {
                
            }
            lastUpdateTime = timeNow;
        }

        #endregion

        public void AddComponent(NumericComponent component)
        {
            Data.Add(component);
        }

        public void RemoveComponent(NumericComponent component)
        {
            Data.Remove(component);
        }
    }
}