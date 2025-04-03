using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class RotateAroundArrangePlugin: ArrangePlugin<ConfigRotateAroundArrange>
    {
        private LinkedList<GameObjectHolder> holders;
        private Quaternion Default;
        protected override void InitInternal()
        {
            holders = model.Holders;
            Default = Quaternion.Euler(Config.ItemEuler);
        }

        public override void Update()
        {
            if (holders == null) return;
            var entity = model.GetParent<Entity>();
            var node = holders.First;
            var angel = 360f / holders.Count;
            float offset = Config.AngleSpeed.Resolve(entity, null) * GameTimerManager.Instance.GetTimeNow() / 1000f % 360;
            float radius = Config.Radius.Resolve(entity, null);
            for (int i = 0; i < holders.Count; i++)
            {
                var holder = node.Value;
                if (holder.EntityView != null)
                {
                    var euler = new Vector3(0, angel * i + offset, 0);
                    if (Config.FollowParentRotation)
                    {
                        holder.EntityView.localRotation = Default * Quaternion.Euler(euler);
                        holder.EntityView.localPosition = holder.EntityView.localRotation * Vector3.forward * radius;
                    }
                    else
                    {
                        var posRot = Default * Quaternion.Euler(euler);
                        var localPos = posRot * Vector3.forward * radius;
                        holder.EntityView.position = model.EntityView.position + localPos;
                        holder.EntityView.rotation = Default * Quaternion.LookRotation(localPos, Vector3.up);
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