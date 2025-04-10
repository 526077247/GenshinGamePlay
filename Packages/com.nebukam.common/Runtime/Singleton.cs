// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


using System;

using System.Reflection;

namespace Nebukam
{

    internal interface ISingleton
    {
        void InternalInit();
    }

    [Flags]
    public enum UpdateFlag
    {
        None = 0,
        Update = 1,
        FixedUpdate = 2,
        LateUpdate = 4,
        All = Update | FixedUpdate | LateUpdate
    }

    public abstract class Singleton<T> : ISingleton
        where T : class, new()
    {

        public static void StaticInitialize() { T i = Get; }

        private static T m_instance = null;

        public static T Get
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new T();

                    ISingleton i = m_instance as ISingleton;
                    i.InternalInit();
                }
                return m_instance;
            }
        }

        private bool m_disposing = false;
        protected bool m_init = false;
        protected UpdateFlag m_updateFlags = UpdateFlag.All;
        protected float m_updateTimeScale = 1f;

        public float updateTimeScale { get => m_updateTimeScale; set => m_updateTimeScale = value; }

        void ISingleton.InternalInit()
        {
            if (m_init) { return; }
            Init();

            Type t = GetType();
            BindingFlags f = BindingFlags.Instance | BindingFlags.NonPublic;
            if (IsOverride(t.GetMethod("Update", f))) { Static.onUpdate(InternalUpdate); }
            if (IsOverride(t.GetMethod("LateUpdate", f))) { Static.onLateUpdate(InternalLateUpdate); }
            if (IsOverride(t.GetMethod("FixedUpdate", f))) { Static.onFixedUpdate(InternalFixedUpdate); }
            if (IsOverride(t.GetMethod("OnApplicationQuit", f))) { Static.onQuit(InternalOnApplicationQuit); }

            m_init = true;
        }

        protected abstract void Init();

        #region InternalMethods

        internal void InternalUpdate(float delta)
        {
            if ((m_updateFlags & UpdateFlag.Update) > 0) { Update(delta * m_updateTimeScale); }
        }

        internal void InternalLateUpdate(float delta)
        {
            if ((m_updateFlags & UpdateFlag.LateUpdate) > 0) { LateUpdate(delta * m_updateTimeScale); }
        }

        internal void InternalFixedUpdate(float delta)
        {
            if ((m_updateFlags & UpdateFlag.FixedUpdate) > 0) { FixedUpdate(delta * m_updateTimeScale); }
        }

        internal void InternalOnApplicationQuit()
        {
            OnApplicationQuit();
            Dispose();
        }

        private static bool IsOverride(MethodInfo m)
        {
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        #endregion

        protected virtual void Update(float delta) { }
        protected virtual void LateUpdate(float delta) { }
        protected virtual void FixedUpdate(float delta) { }
        protected virtual void OnApplicationQuit() { }

        #region System.IDisposable

        protected virtual void Dispose(bool disposing)
        {

            if (!disposing) { return; }

            Static.offUpdate(InternalUpdate);
            Static.offLateUpdate(InternalLateUpdate);
            Static.offFixedUpdate(InternalFixedUpdate);
            Static.offQuit(InternalOnApplicationQuit);

        }

        public void Dispose()
        {
            if (m_disposing) { return; }
            m_disposing = true;
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            System.GC.SuppressFinalize(this);
        }

        #endregion

    }
}
