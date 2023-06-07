using System;

namespace TaoTie
{
    public abstract class FsmClip : IDisposable
    {
        private FsmState state = null;
        protected ConfigFsmClip cfg = null;
        private float starttime = 0.0f;
        private float endtime = 0.0f;
        public bool IsPlaying = false;

        private float length => this.cfg.Length;

        protected Entity actor => this.state.Fsm.Component.GetParent<Entity>();

        #region IDisposable
        public virtual void OnInit(FsmState state, ConfigFsmClip cfg)
        {
            this.state = state;
            this.cfg = cfg;
        }

        public virtual void Dispose()
        {
            this.state = null;
            this.cfg = null;
            ObjectPool.Instance.Recycle(this);
        }
        #endregion

        public void Start(float nowtime)
        {
            this.starttime = nowtime;
            this.endtime = this.starttime + this.cfg.Length;
            IsPlaying = true;
            OnStart();
        }

        public void Update(float nowtime, float elapsetime)
        {
            if (nowtime >= this.endtime)
            {
                Stop();
            }
            else
            {
                OnUpdate(nowtime, elapsetime);
            }
        }

        public void Stop()
        {
            OnStop();
            IsPlaying = false;
        }

        protected abstract void OnStart();
        protected abstract void OnUpdate(float nowtime, float elapsetime);
        protected abstract void OnStop();
    }

    public abstract class FsmClip<T> : FsmClip where T : ConfigFsmClip
    {
        protected T config => cfg as T;
    }
}