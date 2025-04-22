using UnityEngine;

namespace TaoTie
{
    public partial class MoveComponent
    {
        private SceneEntity target;
        private ConfigFollowMove configFollowMove;
        private NumericComponent nc => parent.GetComponent<NumericComponent>();
        private SceneEntity current => GetParent<SceneEntity>();

        public void SetConfigFollowMove(ConfigFollowMove configFollowMove)
        {
            this.configFollowMove = configFollowMove;
        }
        protected void FollowMoveDestroy()
        {
            configFollowMove = null;
            target = null;
        }

        protected bool FollowMoveUpdate()
        {
            if (target == null || configFollowMove == null) return false;
            if (target.IsDispose)
            {
                target = null;
                if (configFollowMove.DestroyOnTargetDispose)
                {
                    parent.Dispose();
                }
                return false;
            }
            var speed = nc.GetAsFloat(NumericType.Speed);
            var dir = (target.Position + target.Rotation * configFollowMove.Offset - current.Position).normalized;
            if (speed > 0)
            {
                current.Position += dir * speed * GameTimerManager.Instance.GetDeltaTime() / 1000;
            }

            if (configFollowMove.ForceFaceToTarget)
            {
                current.Rotation = Quaternion.Euler(dir);
            }

            return true;
        }

        public void SetTarget(SceneEntity sceneEntity)
        {
            this.target = sceneEntity;
        }
    }
}