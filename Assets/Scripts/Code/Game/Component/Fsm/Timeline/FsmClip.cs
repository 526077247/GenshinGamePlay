using System;

namespace TaoTie
{
    public abstract class FsmClip : IDisposable
    {
        public FsmState state = null;
        public ConfigFsmClip cfg = null;
        public float starttime = 0.0f;
        public float endtime = 0.0f;
        public bool isPlaying = false;

        public float length => this.cfg.Length;

        protected Entity _actor => this.state.fsm.Component.GetParent<Entity>();

        #region IRecyclable
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
            isPlaying = true;
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
            isPlaying = false;
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