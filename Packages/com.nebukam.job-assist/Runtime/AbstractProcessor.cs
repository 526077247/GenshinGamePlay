using System;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessor : IDisposable, ILockable
    {

        /// <summary>
        /// Whether this processor is enabled or not.
        /// Note that this property is only accounted for by compounds.
        /// While disabled a Processor can still be found by TryGetFirst, TryGetFirstInCompount & Find
        /// </summary>
        bool enabled { get; set; }

        /// <summary>
        /// User-defined delta multiplier.
        /// </summary>
        float deltaMultiplier { get; set; }

        /// <summary>
        /// Parent compound for this processor, if any
        /// </summary>
        IProcessorCompound compound { get; set; }
        /// <summary>
        /// Index of this processor inside its parent
        /// </summary>
        int compoundIndex { get; set; }

        /// <summary>
        /// Return whether or not this processor' job is scheduled
        /// </summary>
        bool scheduled { get; }
        /// <summary>
        /// Return whether or not this processor' job is completed
        /// </summary>
        bool completed { get; }

        /// <summary>
        /// Return the current IProcessor dependency, if any.
        /// </summary>
        IProcessor procDependency { get; }
        /// <summary>
        /// Return the current JobHandle dependency, if any.
        /// </summary>
        JobHandle currentHandle { get; }

        /// <summary>
        /// Schedule the processor' job if not scheduled already.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn">IProcessor dependency.</param>
        /// <returns></returns>
        JobHandle Schedule(float delta, IProcessor dependsOn = null);
        /// <summary>
        /// Schedule the processor' job if not scheduled already.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dependsOn">JobHandle dependency.</param>
        /// <returns></returns>
        JobHandle Schedule(float delta, JobHandle dependsOn);

        /// <summary>
        /// Completes the job.
        /// </summary>
        void Complete();

        /// <summary>
        /// Completes the job only if it is finished.
        /// Return false if the job hasn't been scheduled.
        /// </summary>
        /// <returns>Whether the job has been completed or not</returns>
        bool TryComplete();

        /// <summary>
        /// Schedules and immediately completes the job
        /// </summary>
        /// <param name="delta"></param>
        void Run(float delta = 0f);

    }

    public abstract class AbstractProcessor : IProcessor
    {

        public float deltaMultiplier { get; set; } = 1.0f;

        protected bool m_locked = false;
        public bool locked { get { return m_locked; } }

        protected IProcessorCompound m_compound = null;
        public IProcessorCompound compound { get { return m_compound; } set { m_compound = value; } }

        public int compoundIndex { get; set; } = -1;

        protected bool m_hasJobHandleDependency = false;
        protected JobHandle m_jobHandleDependency = default(JobHandle);

        protected IProcessor m_procDependency = null;
        public IProcessor procDependency { get { return m_procDependency; } }

        protected JobHandle m_currentHandle;
        public JobHandle currentHandle { get { return m_currentHandle; } }

        protected float m_lockedDelta = 0f;
        protected float m_scaledLockedDelta = 0f;
        protected float m_deltaSum = 0f;

        protected bool m_scheduled = false;
        public bool scheduled { get { return m_scheduled; } }
        public bool completed { get { return m_scheduled ? m_currentHandle.IsCompleted : false; } }

        protected bool m_enabled = true;
        public bool enabled {
            get { return m_enabled; }
            set
            {
                if(m_locked)
                {
                    throw new Exception("You cannot change a processor status while it is locked.");
                }

                m_enabled = value;
            }
        }

#if UNITY_EDITOR
        protected bool m_disposed = false;
#endif

        #region Scheduling

        public JobHandle Schedule(float delta, IProcessor dependsOn = null)
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed Processor ( " + GetType().Name + " ).");
            }
#endif
            m_deltaSum += delta;

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = false;
            m_procDependency = dependsOn;

            Lock();

            OnPrepare();

            m_currentHandle = OnScheduled(m_procDependency);

            return m_currentHandle;

        }

        public JobHandle Schedule(float delta, JobHandle dependsOn)
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Schedule() called on disposed Processor ( " + GetType().Name + " ).");
            }
#endif
            m_deltaSum += delta;

            if (m_scheduled) { return m_currentHandle; }

            m_scheduled = true;
            m_hasJobHandleDependency = true;
            m_jobHandleDependency = dependsOn;
            m_procDependency = null;

            Lock();

            OnPrepare();

            m_currentHandle = OnScheduled(dependsOn);

            return m_currentHandle;
        }

        internal abstract void OnPrepare();

        internal abstract JobHandle OnScheduled(IProcessor dependsOn = null);

        internal abstract JobHandle OnScheduled(JobHandle dependsOn);

        #endregion

        #region Complete & Apply

        /// <summary>
        /// Complete the job.
        /// </summary>
        public void Complete()
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                throw new Exception("Complete() called on disposed Processor ( " + GetType().Name + " ).");
            }
#endif

            if (!m_scheduled) { return; }

            // Complete dependencies

            if (m_hasJobHandleDependency)
                m_jobHandleDependency.Complete();

            m_procDependency?.Complete();

            // Complete self

            OnCompleteBegins();
            
            m_scheduled = false;
            
            OnCompleteEnds();

            Unlock();

        }

        protected abstract void OnCompleteBegins();

        protected abstract void OnCompleteEnds();

        public bool TryComplete()
        {
            if (!m_scheduled) { return false; }
            if (completed)
            {
                Complete();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void Run(float delta = 0f)
        {
            Schedule(delta);
            Complete();
        }

        #endregion

        #region ILockable

        public virtual void Lock()
        {
            if (m_locked) { return; }
            m_lockedDelta = m_deltaSum;
            m_scaledLockedDelta = m_lockedDelta * deltaMultiplier;
            m_deltaSum = 0f;
            InternalLock();
            m_locked = true;
        }

        protected virtual void InternalLock() { }

        public virtual void Unlock()
        {
            if (!m_locked) { return; }
            m_locked = false;
            if (m_scheduled) { Complete(); } //Complete the job for safety
            InternalUnlock();
        }

        protected virtual void InternalUnlock() { }

        #endregion

        #region Hierarchy

        protected bool TryGetFirstInCompound<P>(out P processor, bool deep = false)
            where P : class, IProcessor
        {
            processor = null;
            if (m_compound != null && compoundIndex >= 0)
            {
                //TODO : If compoundIndex == 0, need to go upward in compounds
                return m_compound.TryGetFirst(compoundIndex - 1, out processor, deep);

            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        protected void Dispose(bool disposing)
        {

            if (!disposing) { return; }
#if UNITY_EDITOR
            m_disposed = true;
#endif

            //Complete the job first so we can rid of unmanaged resources.
            if (m_scheduled) { m_currentHandle.Complete(); }

            InternalDispose();

            m_procDependency = null;
            m_scheduled = false;

        }

        protected virtual void InternalDispose() { }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
