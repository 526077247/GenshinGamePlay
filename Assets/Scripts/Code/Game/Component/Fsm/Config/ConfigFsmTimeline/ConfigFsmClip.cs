using System;
using LitJson.Extensions;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigFsmClip
    {
        public float starttime = 0.0f;
        public float length = 0.0f;
        

        public abstract FsmClip CreateClip(FsmState state);
    }

    public abstract class FsmClip : IDisposable
    {
        public FsmState state = null;
        public ConfigFsmClip cfg = null;
        public float starttime = 0.0f;
        public float endtime = 0.0f;
        public bool isPlaying = false;
        [JsonIgnore]
        public float length => this.cfg.length;
        [JsonIgnore]
        protected Entity _actor => this.state.fsm.Component.entityLogic;

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
            this.endtime = this.starttime + this.cfg.length;
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
        
    }
}