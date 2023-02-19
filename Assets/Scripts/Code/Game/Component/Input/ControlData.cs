using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ControlData: IDisposable
    {
        public bool rawIsMoving;
        public bool hasMovedBySelectTarget;
        public float rawJoystickAngle;
        public Vector3 rawTargetDir;
        public Vector3 rawTargetPosition;
        public float rawInputMold; 
        public bool skillButtonsAvaliable;
        public readonly Dictionary<uint, bool> isSkillButtonHold = new Dictionary<uint, bool>();
        public bool isMuteControl;
        public bool isInWalkSpeed;
        public bool jumpThisFrame;

        public static ControlData Create()
        {
            return ObjectPool.Instance.Fetch<ControlData>();
        }

        public void Dispose()
        {
            rawIsMoving = default;
            hasMovedBySelectTarget = default;
            rawJoystickAngle = default;
            rawTargetDir = default;
            rawTargetPosition = default;
            rawInputMold = default;
            skillButtonsAvaliable = default;
            isSkillButtonHold.Clear();
            isMuteControl = default;
            isInWalkSpeed = default;
            jumpThisFrame = default;
        }
    }
}