using Nebukam.ORCA;
using UnityEngine;

namespace TaoTie
{
    public class ORCAAgentComponent: Component, IComponent
    {
        private Actor actor => GetParent<Actor>();
        private Agent agent;
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

        public void SetDir(Vector3 dir)
        {
            agent.prefVelocity = dir;
        }
        
        public Vector3 GetDir()
        {
            return agent.velocity;
        }

        private void OnChangePosition(SceneEntity sceneEntity, Vector3 old)
        {
            if (agent == null) return;
            agent.pos = sceneEntity.Position;
        }
    }
}