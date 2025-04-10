using System.Collections.Generic;
using UnityEngine;
using Nebukam.Common;

namespace Nebukam
{

    public class Static : MonoBehaviour
    {

        public delegate void Callback();
        public delegate void CallbackUpdate(float delta);

        private static List<CallbackUpdate> m_OnUpdate = new List<CallbackUpdate>();
        private static List<CallbackUpdate> m_OnLateUpdate = new List<CallbackUpdate>();
        private static List<CallbackUpdate> m_OnFixedUpdate = new List<CallbackUpdate>();
        private static List<Callback> m_OnQuit = new List<Callback>();

        public static bool quitting = false;

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            GameObject global = new GameObject("Nebukam.Static");
            DontDestroyOnLoad(global);

            global.hideFlags = HideFlags.HideInHierarchy;
            global.AddComponent<Static>();
        }

        public static void onUpdate(CallbackUpdate func) { ON(func, m_OnUpdate); }
        public static void offUpdate(CallbackUpdate func) { OFF(func, m_OnUpdate); }

        public static void onLateUpdate(CallbackUpdate func) { ON(func, m_OnLateUpdate); }
        public static void offLateUpdate(CallbackUpdate func) { OFF(func, m_OnLateUpdate); }

        public static void onFixedUpdate(CallbackUpdate func) { ON(func, m_OnFixedUpdate); }
        public static void offFixedUpdate(CallbackUpdate func) { OFF(func, m_OnFixedUpdate); }

        public static void onQuit(Callback func) { if (quitting) { return; } ON(func, m_OnQuit); }
        public static void offQuit(Callback func) { if (quitting) { return; } OFF(func, m_OnQuit); }

        private static void ON(Callback func, List<Callback> list) { list.TryAddOnce(func); }
        private static void OFF(Callback func, List<Callback> list) { list.TryRemove(func); }

        private static void ON(CallbackUpdate func, List<CallbackUpdate> list) { list.TryAddOnce(func); }
        private static void OFF(CallbackUpdate func, List<CallbackUpdate> list) { list.TryRemove(func); }

        private void Update() { Call(m_OnUpdate, Time.deltaTime); }
        private void LateUpdate() { Call(m_OnLateUpdate, Time.deltaTime); }
        private void FixedUpdate() { Call(m_OnFixedUpdate, Time.fixedDeltaTime); }
        private void OnApplicationQuit()
        {
            quitting = true;
            Call(m_OnQuit);
        }

        private void Call(List<Callback> list) { for (int i = 0, count = list.Count; i < count; i++) { list[i](); } }
        private void Call(List<CallbackUpdate> list, float delta) { for (int i = 0, count = list.Count; i < count; i++) { list[i](delta); } }

    }

}
