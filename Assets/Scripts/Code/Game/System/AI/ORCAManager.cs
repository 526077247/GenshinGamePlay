using Nebukam.Common;
using Nebukam.ORCA;
using UnityEngine;

namespace TaoTie
{
    public class ORCAManager: IManager, IUpdate
    {
        private ORCABundle<Agent> bundle;

        public void Init()
        {
            bundle = new ORCABundle<Agent>();
            bundle.plane = AxisPair.XZ;
        }

        public void Destroy()
        {
            
            if (bundle != null)
            {
                bundle.Dispose();
                bundle = null;
            }
        }

        public Agent AddEntity(SceneEntity sceneEntity)
        {
            var agent = bundle.NewAgent(sceneEntity.Position);
            return agent;
        }

        public void Update()
        {
            if (bundle.orca.TryComplete())
            {
                bundle.orca.Schedule(GameTimerManager.Instance.GetDeltaTime() / 1000f);
            }
            else
            {
                bundle.orca.Schedule(GameTimerManager.Instance.GetDeltaTime() / 1000f);
            }
        }
    }
}