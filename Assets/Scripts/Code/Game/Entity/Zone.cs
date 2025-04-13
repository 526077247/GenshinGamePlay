using UnityEngine;

namespace TaoTie
{
    public class Zone: SceneEntity,IEntity<ConfigShape>
    {
        #region IEntity

        public override EntityType Type => EntityType.Zone;

        public ConfigShape Config;

        public void Init(ConfigShape shape)
        {
            Config = shape;
            AddComponent<TriggerComponent, ConfigShape, int>(shape, 200);
        }

        public void Destroy()
        {
            Config = null;
        }

        #endregion

        public float GetSqrDistance(Vector3 target)
        {
            return Config.SqrMagnitude(target - Position);
        }
        
        public bool Contains(Vector3 target)
        {
            return Config.Contains(target);
        }
        
        public Vector3 GetCenter()
        {
            return Position;
        }
    }
}