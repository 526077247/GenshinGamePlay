#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class AIDebug: MonoBehaviour
    {
        public static AIDebug Show;
        public string Act;
        public string Tactic;
        public string Move;

        private void Awake()
        {
            if(Show == null) Show = this;
        }

        [Button("显示在GUI")]
        public void ShowInGUI()
        {
            Show = this;
        }

        private void OnGUI()
        {
            if (Show != this) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 32;
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(10, 10, 200, 90), "Act: " + Act, style);
            GUI.Label(new Rect(10, 50, 200, 90), "Tactic: " + Tactic, style);
            GUI.Label(new Rect(10, 90, 200, 90), "Move: " + Move, style);
        }
    }
}

#endif