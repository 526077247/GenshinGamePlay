using UnityEngine;

namespace TaoTie
{
    public class ORCAAgentComponent : Component, IComponent
    {
        private Actor actor => GetParent<Actor>();
        private ORCASystem system;
        private ORCASystem.Slot slot;
        private bool rvoEnabled;
        private Vector3 prefVelocity;

        public void Init()
        {
            rvoEnabled = false;
            if (actor?.ConfigActor?.Common != null && SceneManager.Instance.CurrentScene is MapScene scene)
            {
                system = scene.GetManager<ORCASystem>();
                if (system == null) return;
                float radius = actor.ConfigActor.Common.ModelRadius;
                float height = actor.ConfigActor.Common.ModelHeight;
                slot = system.AddEntity(actor.Position, radius, height);
                if (slot != null)
                    slot.enabled = false;
            }
            Messager.Instance.AddListener<SceneEntity, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<SceneEntity, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            if (slot != null && system != null)
                system.RemoveEntity(slot.id);
            slot = null;
            system = null;
        }

        public void EnableRVO2(bool enable)
        {
            rvoEnabled = enable;
            if (slot != null)
                slot.enabled = enable;
        }

        public void SetVelocity(Vector3 velocity, float maxSpeed)
        {
            prefVelocity = velocity;
            if (slot != null)
            {
                slot.prefVelocity = velocity;
                slot.maxSpeed = maxSpeed;
            }
        }

        public Vector3 GetVelocity()
        {
            if (slot == null || !rvoEnabled)
                return prefVelocity;
            return slot.velocity;
        }

        private void OnChangePosition(SceneEntity sceneEntity, Vector3 old)
        {
            if (slot != null)
                slot.position = sceneEntity.Position;
        }
    }
}
