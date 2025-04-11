using Nebukam.ORCA;
using UnityEngine;

namespace TaoTie
{
    public class ORCAAgentComponent: Component, IComponent
    {
        private Actor actor => GetParent<Actor>();
        private Agent agent;
        private Vector3 prefVelocity;
        public void Init()
        {
            if (actor?.ConfigActor?.Common != null && SceneManager.Instance.CurrentScene is MapScene scene)
            {
                var orcaManager = scene.GetManager<ORCAManager>();
                if(orcaManager==null) return;
                agent = orcaManager.AddEntity(actor);
                agent.navigationEnabled = false;
                agent.height = actor.ConfigActor.Common.ModelHeight;
                agent.radius = actor.ConfigActor.Common.ModelRadius;
            }
            Messager.Instance.AddListener<SceneEntity, Vector3>(Id,MessageId.ChangePositionEvt,OnChangePosition);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<SceneEntity, Vector3>(Id,MessageId.ChangePositionEvt,OnChangePosition);
            if (agent != null)
            {
                agent.Release();
                agent = null;
            }
        }

        public void EnableRVO2(bool enable)
        {
            agent.navigationEnabled = enable;
        }

        public void SetVelocity(Vector3 velocity, float maxSpeed)
        {
            prefVelocity = velocity;
            if (agent == null) return;
            agent.prefVelocity = prefVelocity;
            agent.maxSpeed = maxSpeed;
        }
        
        public Vector3 GetVelocity()
        {
            if (agent == null || !agent.navigationEnabled) return prefVelocity;
            return agent.velocity;
        }

        private void OnChangePosition(SceneEntity sceneEntity, Vector3 old)
        {
            if (agent == null) return;
            agent.pos = sceneEntity.Position;
        }
    }
}