using UnityEngine;

namespace TaoTie
{
    public class Zone: Entity,IEntity
    {
        #region IEntity

        public override EntityType Type => EntityType.Zone;

        public GameObject GameObject;
        public void Init()
        {
            GameObject = new GameObject("Zone");
            GameObject.layer = LayerMask.NameToLayer("Entity");
        }

        public void Destroy()
        {
            if (GameObject != null)
            {
                GameObject.Destroy(GameObject);
                GameObject = null;
            }
        }

        #endregion
    }
}