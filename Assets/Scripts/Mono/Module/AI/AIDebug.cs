#if UNITY_EDITOR
using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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
        public bool HasLineOfSight;
        public List<Vector3> PathPoints;
        public Vector3? EyePos;
        public Vector3? TargetTopPos;

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
            GUI.Label(new Rect(10, 250, 200, 90), "HasLineOfSight: " + HasLineOfSight, style);
            GUI.Label(new Rect(10, 290, 200, 90), "HasPath: " + (PathPoints?.Count > 0), style);
        }

        private void OnDrawGizmos()
        {
            if (TargetPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere((Vector3)TargetPos, 0.1f);
            }

            if (EyePos != null && TargetPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(EyePos.Value, TargetPos.Value);
                if (TargetTopPos != null)
                    Gizmos.DrawLine(EyePos.Value, TargetTopPos.Value);
            }

            if (ViewRange > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, ViewRange);
            }

            if (PathPoints != null && PathPoints.Count > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < PathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(PathPoints[i], PathPoints[i + 1]);
                }
            }
        }
    }
}
#else
using System.Collections.Generic;
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
        public bool HasLineOfSight;
        public List<Vector3> PathPoints;
        public Vector3? EyePos;
        public Vector3? TargetTopPos;
    }
}
#endif