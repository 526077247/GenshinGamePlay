using Unity.Collections;
using Unity.Jobs;
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.JobAssist
{

    public interface IProcessorGroup : IProcessorCompound
    {

    }

    /// <summary>
    /// A ProcessorGroup starts its child processors at the same time 
    /// and return a combined handle
    /// </summary>
    public abstract class ProcessorGroup : AbstractProcessorCompound, IProcessorGroup
    {

        protected NativeArray<JobHandle> m_groupHandles = default;

        #region Scheduling

        internal override void OnPrepare()
        {
            MakeLength(ref m_groupHandles, m_enabledChildren);
            base.OnPrepare();
        }

        internal override JobHandle OnScheduled(IProcessor dependsOn = null)
        {

            if (m_isCompoundEmpty) { return ScheduleEmpty(dependsOn); }

            int count = Count;
            IProcessor proc;
            IProcessorCompound comp;

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                comp = proc as IProcessorCompound;
                if (!proc.enabled
                    || (comp != null && comp.isCompoundEmpty))
                { continue; } // Skip disabled and/or empty

                m_groupHandles[i] = proc.Schedule(m_scaledLockedDelta, dependsOn);
            }

            return JobHandle.CombineDependencies(m_groupHandles);

        }

        internal override JobHandle OnScheduled(JobHandle dependsOn)
        {

            if (m_isCompoundEmpty) { return ScheduleEmpty(dependsOn); }

            int count = Count;
            IProcessor proc;
            IProcessorCompound comp;

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                comp = proc as IProcessorCompound;
                if (!proc.enabled
                    || (comp != null && comp.isCompoundEmpty))
                { continue; } // Skip disabled and/or empty

                m_groupHandles[i] = proc.Schedule(m_scaledLockedDelta, dependsOn);
            }

            return JobHandle.CombineDependencies(m_groupHandles);

        }

        #endregion

        #region Complete & Apply

        protected sealed override void OnCompleteEnds() { }
        
        #endregion

        #region IDisposable

        protected override void InternalDispose()
        {
            m_groupHandles.Release();
        }

        #endregion

        #region Abstracts
        /*
        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }
        */
        #endregion

    }
}
