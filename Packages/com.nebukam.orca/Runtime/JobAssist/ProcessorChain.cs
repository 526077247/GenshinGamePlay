using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessorChain : IProcessorCompound
    {

    }

    /// <summary>
    /// A ProcessorChain chains its child processor and return the last
    /// job of the chain as its handle.
    /// </summary>
    public abstract class ProcessorChain : AbstractProcessorCompound, IProcessorChain
    {


        #region Scheduling

        internal override JobHandle OnScheduled(IProcessor dependsOn = null)
        {

            if (m_isCompoundEmpty) { return ScheduleEmpty(dependsOn); }

            int count = m_childs.Count;
            IProcessor proc, prevProc = dependsOn;
            IProcessorCompound comp;
            JobHandle handle = default(JobHandle);

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                proc.compoundIndex = i; // Redundant ?
                comp = proc as IProcessorCompound;

                if (!proc.enabled
                    || (comp != null && comp.isCompoundEmpty)) 
                { continue; } // Skip disabled and/or empty



                handle = prevProc == null 
                    ? proc.Schedule(m_scaledLockedDelta) 
                    : proc.Schedule(m_scaledLockedDelta, prevProc); ;
                prevProc = proc;

            }

            return handle;

        }

        internal override JobHandle OnScheduled(JobHandle dependsOn)
        {

            if (m_isCompoundEmpty) { return ScheduleEmpty(dependsOn); }

            int count = m_childs.Count;
            IProcessor proc, prevProc = null;
            IProcessorCompound comp;
            JobHandle handle = default(JobHandle);

            for (int i = 0; i < count; i++)
            {
                proc = m_childs[i];
                proc.compoundIndex = i; // Redundant ?

                comp = proc as IProcessorCompound;

                if (!proc.enabled
                    || (comp != null && comp.isCompoundEmpty))
                { continue; } // Skip disabled and/or empty

                handle = prevProc == null 
                    ? proc.Schedule(m_scaledLockedDelta, m_jobHandleDependency) 
                    : proc.Schedule(m_scaledLockedDelta, prevProc);
                prevProc = proc;

            }

            return handle;
        }

        #endregion

        #region Complete & Apply

        protected sealed override void OnCompleteEnds() { }

        #endregion

        #region Abstracts
        /*
        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }
        */
        #endregion

    }
}
