using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    public interface IProcessorCompound : IProcessor
    {

        /// <summary>
        /// Return the current number of children in this compound
        /// </summary>
        int Count { get; }

        bool isCompoundEmpty { get; }

        /// <summary>
        /// Return the child stored at a given index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        IProcessor this[int i] { get; }

        /// <summary>
        /// Dispose of the compound as well as all of its childrens. 
        /// Recursive.
        /// </summary>
        void DisposeAll();

        /// <summary>
        /// Attempt to find the first item of type P
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="startIndex"></param>
        /// <param name="processor"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        bool TryGetFirst<P>(int startIndex, out P processor, bool deep = false) where P : class, IProcessor;
        bool Find<P>(out P processor) where P : class, IProcessor;

    }

    public abstract class AbstractProcessorCompound : AbstractProcessor, IProcessorCompound
    {

        protected EmptyCompound m_emptyCompoundJob;
        protected bool m_isCompoundEmpty = false;
        public bool isCompoundEmpty { get { return m_isCompoundEmpty; } }

        protected int m_enabledChildren = 0;

        protected List<IProcessor> m_childs = new List<IProcessor>();
        public int Count { get { return m_childs.Count; } }

        public IProcessor this[int i] { get { return m_childs[i]; } }

        #region Child management

        public IProcessor Add(IProcessor proc)
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif

            if (m_childs.Contains(proc)) { return proc; }
            m_childs.Add(proc);
            return OnChildAdded(proc, Count - 1);
        }

        public P Add<P>(P proc)
            where P : class, IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif

            if (m_childs.Contains(proc)) { return proc; }
            m_childs.Add(proc);
            return OnChildAdded(proc, Count - 1) as P;
        }

        /// <summary>
        /// Create (if null) and add item
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="proc"></param>
        /// <returns></returns>
        public P Add<P>(ref P proc)
            where P : class, IProcessor, new()
        {
#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot add new processors to a locked chain");
            }
#endif
            if (proc != null) { return Add(proc); }
            proc = new P();
            return Add(proc);
        }

        public P Insert<P>(int atIndex, P proc)
            where P : class, IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot insert new processors to a locked chain");
            }
#endif
            if (m_childs.Contains(proc)) { return proc; } //TODO: Handle situation gracefully, re-order ?
            if (atIndex > m_childs.Count - 1) { return Add(proc); }

            m_childs.Insert(atIndex, proc);
            return OnChildAdded(proc, atIndex) as P;
        }

        public P InsertBefore<P>(IProcessor beforeItem, P proc)
            where P : class, IProcessor
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot insert new processors to a locked chain");
            }
#endif
            int atIndex = m_childs.IndexOf(beforeItem);
            if (atIndex == -1) { return Add(proc); }
            return Insert(atIndex, proc);
        }

        public P InsertBefore<P>(IProcessor beforeProc, ref P proc)
            where P : class, IProcessor, new()
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot insert new processors to a locked chain");
            }
#endif
            if (proc != null) { return InsertBefore(beforeProc, proc); }
            proc = new P();
            return InsertBefore(beforeProc, proc);
        }

        /// <summary>
        /// Removes a processor from the chain
        /// </summary>
        /// <param name="proc"></param>
        public IProcessor Remove(IProcessor proc)
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot remove processors from a locked chain");
            }
#endif

            int index = m_childs.IndexOf(proc);
            if (index == -1) { return null; }

            m_childs.RemoveAt(index);
            return OnChildRemoved(proc, index);

        }

        /// <summary>
        /// Removes a processor from the chain
        /// </summary>
        /// <param name="proc"></param>
        public IProcessor RemoveAt(int index)
        {

#if UNITY_EDITOR
            if (m_locked)
            {
                throw new Exception("You cannot remove processors from a locked chain");
            }
#endif
            IProcessor proc = m_childs[index];
            m_childs.RemoveAt(index);
            return OnChildRemoved(proc, index);

        }


        protected IProcessor OnChildAdded(IProcessor child, int childIndex)
        {
            child.compound = this;
            RefreshChildIndices(childIndex);
            return child;
        }

        protected IProcessor OnChildRemoved(IProcessor child, int childIndex)
        {
            child.compound = null;
            RefreshChildIndices(childIndex);
            return child;
        }

        protected void RefreshChildIndices(int from)
        {
            int count = Count;
            if (from <= count - 1)
            {
                for (int i = from; i < count; i++)
                    m_childs[i].compoundIndex = i;
            }
        }

        #endregion

        #region Scheduling

        internal override void OnPrepare()
        {
            Prepare(m_scaledLockedDelta);
        }

        internal JobHandle ScheduleEmpty(IProcessor dependsOn = null)
        {
            return dependsOn == null
                ? m_emptyCompoundJob.Schedule()
                : m_emptyCompoundJob.Schedule(dependsOn.currentHandle);
        }

        internal JobHandle ScheduleEmpty(JobHandle dependsOn)
        {
            return m_emptyCompoundJob.Schedule(dependsOn);
        }

        /// <summary>
        /// In a ProcessorGroup, Prepare is called right before scheduling the existing group for the job.
        /// If you intend to dynamically modify the group childs list, do so in InternalLock(), right before base.InternalLock().
        /// </summary>
        /// <param name="delta"></param>
        protected virtual void Prepare(float delta) { }

        #endregion

        #region Complete & Apply

        protected sealed override void OnCompleteBegins()
        {

            if (m_isCompoundEmpty)
            {
                m_currentHandle.Complete();
            }
            else
            {
                for (int i = 0, n = m_childs.Count; i < n; i++)
                    m_childs[i].Complete();
            }

            Apply();

        }

        protected virtual void Apply() { }

        #endregion

        #region ILockable

        public override sealed void Lock()
        {

            if (m_locked) { return; }

            base.Lock();

            m_enabledChildren = 0;

            for (int i = 0, n = m_childs.Count; i < n; i++)
            {
                IProcessor child = m_childs[i];
                child.Lock();

                if (!child.enabled) { continue; }

                // Skip empty compounds
                IProcessorCompound childCompound = child as IProcessorCompound;
                if (childCompound != null && childCompound.isCompoundEmpty) { continue; }

                m_enabledChildren++;
            }

            if (m_enabledChildren == 0)
            {
                m_isCompoundEmpty = true;
                m_emptyCompoundJob = default;
            }
            else
            {
                m_isCompoundEmpty = false;
            }

        }

        public override sealed void Unlock()
        {
            if (!m_locked) { return; }

            base.Unlock();

            for (int i = 0, n = m_childs.Count; i < n; i++)
                m_childs[i].Unlock();
        }

        #endregion

        #region Hierarchy

        public bool TryGetFirst<P>(int startIndex, out P processor, bool deep = false)
            where P : class, IProcessor
        {

            processor = null;

            if (startIndex < 0) { startIndex = m_childs.Count - 1; }

            IProcessor child;
            IProcessorCompound childCompound;

            for (int i = startIndex; i >= 0; i--)
            {
                child = m_childs[i];
                processor = child as P;

                if (processor != null)
                {
                    return true;
                }

                if (!deep) { continue; }

                childCompound = child as IProcessorCompound;

                if (childCompound != null
                    && childCompound.Find(out processor))
                    return true;

            }

            //If local search fails, it goes up one level and restarts.
            //This is actually super slow so make sure to cache the results of TryGet & Find.
            return TryGetFirstInCompound(out processor, deep);

        }

        /// <summary>
        /// Goes through all child processors & compounts in reverse order
        /// until if find a processor with the correct signature.
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="processor"></param>
        /// <returns></returns>
        public bool Find<P>(out P processor)
            where P : class, IProcessor
        {

            processor = null;

            IProcessor child;
            IProcessorCompound childCompound;

            for (int i = Count - 1; i >= 0; i--)
            {
                child = m_childs[i];
                processor = child as P;

                if (processor != null)
                    return true;

                childCompound = child as IProcessorCompound;

                if (childCompound != null
                    && childCompound.Find(out processor))
                    return true;

            }

            return false;

        }

        #endregion

        #region IDisposable

        public void DisposeAll()
        {

#if UNITY_EDITOR
            if (m_disposed)
            {
                return;

                //throw new Exception("DisposeAll() called on already disposed Compound.");
            }
#endif

            if (m_scheduled) { m_currentHandle.Complete(); }

            IProcessor p;

            for (int i = 0, count = m_childs.Count; i < count; i++)
            {
                p = m_childs[i];

                if (p is IProcessorCompound)
                    (p as IProcessorCompound).DisposeAll();
                else
                    p.Dispose();

            }

            m_scheduled = false; // Avoid Completting current handle twice

            Dispose();

        }

        #endregion

    }
}
