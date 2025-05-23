﻿#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class AIDebug: MonoBehaviour
    {
        private GUIStyle style = new GUIStyle();
        
        private static AIDebug Show;
        public string Act;
        public string Tactic;
        public string Move;
        public string Target;
        public Vector3? TargetPos;
        public float ViewRange;
        public string Alertness;
        public string SkillStatus;

        private void Awake()
        {
            style.fontSize = 32;
            style.normal.textColor = Color.red;
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
            
            GUI.Label(new Rect(10, 10, 200, 90), "Act: " + Act, style);
            GUI.Label(new Rect(10, 50, 200, 90), "Tactic: " + Tactic, style);
            GUI.Label(new Rect(10, 90, 200, 90), "Move: " + Move, style);
            GUI.Label(new Rect(10, 130, 200, 90), "Target: " + Target, style);
            GUI.Label(new Rect(10, 170, 200, 90), "Alertness: " + Alertness, style);
            GUI.Label(new Rect(10, 210, 200, 90), "SkillStatus: " + SkillStatus, style);
        }

        private void OnDrawGizmos()
        {
            if (TargetPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere((Vector3)TargetPos, 0.1f);
            }

            if (ViewRange > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, ViewRange);
            }
        }
    }
}
#else
using UnityEngine;

namespace TaoTie
{
    public class AIDebug: MonoBehaviour
    {
        public string Act;
        public string Tactic;
        public string Move;
        public string Target;
        public Vector3? TargetPos;
        public float ViewRange;
        public string Alertness;
        public string SkillStatus;
    }
}
#endif