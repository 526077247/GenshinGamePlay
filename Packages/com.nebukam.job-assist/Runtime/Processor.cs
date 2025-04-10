using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public abstract class Processor<T> : AbstractProcessor, IProcessor
        where T : struct, IJob
    {

        protected T m_currentJob;

        #region Scheduling

        internal override void OnPrepare()
        {
            Prepare(ref m_currentJob, m_scaledLockedDelta);
        }

        internal override JobHandle OnScheduled(IProcessor dependsOn = null)
        {
            return dependsOn == null 
                ? m_currentJob.Schedule() 
                : m_currentJob.Schedule(dependsOn.currentHandle);
        }

        internal override JobHandle OnScheduled(JobHandle dependsOn)
        {
            return m_currentJob.Schedule(dependsOn);
        }

        protected abstract void Prepare(ref T job, float delta);

        #endregion

        #region Complete & Apply

        protected sealed override void OnCompleteBegins()
        {
            m_currentHandle.Complete();
        }

        protected sealed override void OnCompleteEnds()
        {
            Apply(ref m_currentJob);
        }

        protected virtual void Apply(ref T job) { }

        #endregion

        #region ILockable

        public sealed override void Lock()
        {
            if (m_locked) { return; }
            m_currentJob = default;
            base.Lock();
        }

        public sealed override void Unlock(){ 
            base.Unlock(); 
        }

        #endregion

    }

}
