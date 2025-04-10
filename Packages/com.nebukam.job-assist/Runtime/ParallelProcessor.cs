using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IParallelProcessor : IProcessor
    {
        int chunkSize { get; set; }
    }


    public abstract class ParallelProcessor<T> : AbstractProcessor, IParallelProcessor
        where T : struct, IJobParallelFor
    {

        protected T m_currentJob;
        public int chunkSize { get; set; } = 64;

        protected int m_jobLength = 0;

        #region Scheduling

        internal override void OnPrepare()
        {
            m_jobLength = Prepare(ref m_currentJob, m_scaledLockedDelta);
        }

        internal override JobHandle OnScheduled(IProcessor dependsOn = null)
        {
            return dependsOn == null
                ? m_currentJob.Schedule(m_jobLength, chunkSize)
                : m_currentJob.Schedule(m_jobLength, chunkSize, dependsOn.currentHandle);
        }

        internal override JobHandle OnScheduled(JobHandle dependsOn)
        {
            return m_currentJob.Schedule(m_jobLength, chunkSize, dependsOn);
        }

        protected abstract int Prepare(ref T job, float delta);

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

        public sealed override void Unlock()
        {
            base.Unlock();
        }

        #endregion

    }

}
