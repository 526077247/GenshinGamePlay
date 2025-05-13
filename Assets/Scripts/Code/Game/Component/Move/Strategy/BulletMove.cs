using UnityEngine;

namespace TaoTie
{
    public partial class BulletMove: MoveStrategy<ConfigBulletMove>
    {
        private NumericComponent nc => parent.GetComponent<NumericComponent>();
        public override bool OverrideUpdate => false;
        public bool IsStart { get;private set; }
        /// <summary>
        /// 延时启动的间隔时间
        /// </summary>
        private int delay;

        /// <summary>
        /// 延时到什么时间启动
        /// </summary>
        private long delayTillTime;

        private long startTime;

        private float acceleration;
        
        protected override void InitInternal()
        {
            IsStart = false;
            delay = config.Delay;
            if (delay >= 0)
            {
                OnStart();
            }
        }

        protected override void UpdateInternal()
        {
            long nowtime = GameTimerManager.Instance.GetTimeNow();
            
            #region 延迟启动
            int delay = this.delay;
            if (this.delay > 0 && delayTillTime <= 0)
            {
                delayTillTime = nowtime + delay;
            }

            if (nowtime < delayTillTime)
            {
                return;
            }
            
            delayTillTime = 0;
            this.delay = 0;
            #endregion
            OnStart();
            float speed;
            if (config.AccelerationTime < 0)
            {
                speed = config.Speed + (nowtime - startTime) * acceleration;
            }
            else
            {
                speed = config.Speed + Mathf.Min(config.AccelerationTime, nowtime - startTime) * acceleration;
            }
            speed = Mathf.Clamp(speed, config.MinSpeed, config.MaxSpeed);
            nc.Set(NumericType.SpeedBase, speed);
            moveComponent.CharacterInput.Direction = parent.Forward.normalized;
            moveComponent.CharacterInput.RotateSpeed = config.AnglerVelocity.Resolve(parent, null);
            moveComponent.CharacterInput.MotionDirection = MotionDirection.Forward;
        }

        private void OnStart()
        {
            if (this.IsStart) return;
            IsStart = true;

            delayTillTime = 0;
            delay = 0;
            acceleration = config.Acceleration.Resolve(parent, null);
            startTime = GameTimerManager.Instance.GetTimeNow();
        }

        protected override void DisposeInternal()
        {
            IsStart = false;
            delay = 0;
            delayTillTime = 0;
            startTime = 0;
            acceleration = 0;
        }
    }
}