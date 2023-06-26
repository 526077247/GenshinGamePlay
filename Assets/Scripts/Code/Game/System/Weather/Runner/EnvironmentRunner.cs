using System;

namespace TaoTie
{
    public abstract class EnvironmentRunner:IPriorityStackItem, IDisposable
    {
        public int Priority { get; protected set; }
        public long Id { get; protected set; }
        public bool IsOver;

        public EnvironmentInfo Data;

        protected WeatherSystem weatherSystem;
        
        public virtual void Update(){}

        public abstract void Dispose();
    }
}