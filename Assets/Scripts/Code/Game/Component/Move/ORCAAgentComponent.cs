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
            if (actor?.ConfigActor?.Common != null && SceneManager.Instance.CurrentScene is SceneManagerProvider scene)
            {
                system = scene.GetManager<ORCASystem>();
                if (system == null) return;
                float radius = actor.ConfigActor.Common.ModelRadius;
                float height = actor.ConfigActor.Common.ModelHeight;
                slot = system.AddEntity(actor.Position, radius, height);
                if (slot != null)
                {
                    slot.enabled = false;
                    slot.baseline = actor.Position.y;
                }
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

        /// <summary>
        /// RVO2 避障是否已开启。
        /// </summary>
        public bool IsRVOEnabled => rvoEnabled;

        /// <summary>当前场景是否注册了可碰撞的静态障碍（河流/陡坡/feature 等）。</summary>
        public bool HasObstacles => system != null && system.HasObstacles;

        /// <summary>当前场景是否注册了水面障碍（河流水面足印）。</summary>
        public bool HasWaterObstacles => system != null && system.HasWaterObstacles;

        /// <summary>检测给定圆是否与水面障碍（河流水面足印）重叠。</summary>
        public bool OverlapsWaterObstacle(Vector3 position, float radius, float baseline, float height)
        {
            return system != null && system.OverlapsWaterObstacle(position, radius, baseline, height);
        }

        /// <summary>
        /// 检测位置是否与 ORCA 静态障碍相交（出生点避障用）。
        /// </summary>
        public bool OverlapsObstacle(Vector3 position, float radius, float baseline, float height)
        {
            return system != null && system.OverlapsObstacle(position, radius, baseline, height);
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
            {
                slot.position = sceneEntity.Position;
                slot.baseline = sceneEntity.Position.y;
            }
        }
    }
}
