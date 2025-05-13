using UnityEngine;

namespace TaoTie
{
    public class FollowMove: MoveStrategy<ConfigFollowMove>
    {
        public override bool OverrideUpdate => true;
        private NumericComponent nc => parent.GetComponent<NumericComponent>();

        protected override void UpdateInternal()
        {
            if (target == null || config == null) return;
            if (target.IsDispose)
            {
                target = null;
                if (config.DestroyOnTargetDispose)
                {
                    parent.Dispose();
                }
                return;
            }
            var speed = nc.GetAsFloat(NumericType.Speed);
            var dir = (target.Position + target.Rotation * config.Offset - parent.Position).normalized;
            if (speed > 0)
            {
                parent.Position += dir * speed * GameTimerManager.Instance.GetDeltaTime() / 1000;
            }

            if (config.ForceFaceToTarget)
            {
                parent.Rotation = Quaternion.Euler(dir);
            }
        }
        
    }
}