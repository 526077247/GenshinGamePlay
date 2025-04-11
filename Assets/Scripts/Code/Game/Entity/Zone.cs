using UnityEngine;

namespace TaoTie
{
    public class Zone: Entity,IEntity
    {
        #region IEntity

        public override EntityType Type => EntityType.Zone;

        public GameObject GameObject;
        public Collider Collider;
        public Mesh DynMesh;
        public void Init()
        {
            GameObject = new GameObject("Zone");
            GameObject.layer = LayerMask.NameToLayer("Entity");
        }

        public void Destroy()
        {
            if (DynMesh != null)
            {
                GameObject.Destroy(DynMesh);
                DynMesh = null;
            }
            Collider = null;
            if (GameObject != null)
            {
                GameObject.Destroy(GameObject);
                GameObject = null;
            }
        }

        #endregion

        public float GetSqrDistance(Vector3 target)
        {
            if (GameObject == null) return 0;
            if (Collider == null) return Vector3.SqrMagnitude(target - GameObject.transform.position);
            if (Collider.bounds.Contains(target))
            {
                return 0;
            }

            return Collider.bounds.SqrDistance(target);
        }
        
        public bool CheckInZone(Vector3 target)
        {
            if (GameObject == null) return false;
            if (Collider == null) return false;
            return Collider.bounds.Contains(target);
        }
        
        public Vector3 GetCenter()
        {
            if (GameObject == null) return Vector3.zero;
            if (Collider == null) return GameObject.transform.position;
            return Collider.bounds.center;
        }
    }
}