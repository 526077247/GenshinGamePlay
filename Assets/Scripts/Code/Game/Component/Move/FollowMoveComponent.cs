using UnityEngine;

namespace TaoTie
{
    public class FollowMoveComponent: Component, IComponent<ConfigFollowMove>, IUpdate
    {
        private SceneEntity target;
        private ConfigFollowMove configFollowMove;
        private NumericComponent nc => parent.GetComponent<NumericComponent>();
        private SceneEntity current => GetParent<SceneEntity>();
        public void Init(ConfigFollowMove config)
        {
            configFollowMove = config;
        }

        public void Destroy()
        {
            configFollowMove = null;
            target = null;
        }

        public void Update()
        {
            if (target == null || configFollowMove == null) return;
            if (target.IsDispose)
            {
                target = null;
                if (configFollowMove.DestroyOnTargetDispose)
                {
                    parent.Dispose();
                }
                return;
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
        }

        public void SetTarget(SceneEntity sceneEntity)
        {
            this.target = sceneEntity;
        }
    }
}