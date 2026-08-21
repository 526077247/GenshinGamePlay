using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class RotateAroundArrangePlugin: ArrangePlugin<ConfigRotateAroundArrange>
    {
        private LinkedList<GameObjectHolder> holders;

        private Vector3 up;
        private Vector3 forward;
        protected override void InitInternal()
        {
            holders = UnitModel.Holders;
            switch (Config.RotAngleType)
            {
                case RotAngleType.ROT_ANGLE_X:
                    up = Vector3.right;
                    forward = Vector3.up;
                    break;
                case RotAngleType.ROT_ANGLE_Y:
                    up = Vector3.up;
                    forward = Vector3.forward;
                    break;
                case RotAngleType.ROT_ANGLE_Z:
                    up = Vector3.back;
                    forward = Vector3.up;
                    break;
            }
        }

        public override void Update()
        {
            if (holders == null) return;
            var entity = UnitModel.GetParent<Entity>();
            var node = holders.First;
            var angle = 360f / holders.Count;
            float offset = Config.AngleSpeed.Resolve(entity, null) * GameTimerManager.Instance.GetTimeNow() / 1000f % 360;
            float radius = Config.Radius.Resolve(entity, null);
            for (int i = 0; i < holders.Count; i++)
            {
                var holder = node.Value;
                if (holder.EntityView != null)
                {
                    var euler = up * (angle * i + offset);
                    if (Config.FollowParentRotation)
                    {
                        holder.EntityView.localRotation = Quaternion.Euler(euler);
                        holder.EntityView.localPosition = holder.EntityView.localRotation * forward * radius;
                    }
                    else
                    {
                        var posRot = Quaternion.Euler(euler);
                        var localPos = posRot * forward * radius;
                        holder.EntityView.position = UnitModel.EntityView.position + localPos;
                        holder.EntityView.rotation = Quaternion.LookRotation(localPos, up);
                    }
                    
                }
                node = node.Next;
            }
        }

        protected override void DisposeInternal()
        {
            holders = null;
        }
        
    }
}