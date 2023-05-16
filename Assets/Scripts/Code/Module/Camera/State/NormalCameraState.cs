using System.Collections.Generic;

namespace TaoTie
{
    public class NormalCameraState: CameraState
    {
        public override bool IsBlenderState => false;
        public ConfigCamera Config { get; private set; }
        
        private CameraPluginRunner body;
        private CameraPluginRunner head;
        private ListComponent<CameraPluginRunner> others;

        public static NormalCameraState Create(ConfigCamera config)
        {
            NormalCameraState res = ObjectPool.Instance.Fetch<NormalCameraState>();
            res.Config = config;
            res.Data = CameraStateData.Create();
            res.CreateRunner();
            return res;
        }

        private void CreateRunner()
        {
            if (Config.HeadPlugin != null)
            {
                head = RunnerFactory.CreateHeadPluginRunner(Config.HeadPlugin);
            }

            if (Config.BodyPlugin != null)
            {
                body = RunnerFactory.CreateBodyPluginRunner(Config.BodyPlugin);
            }
            
            if (Config.OtherPlugin != null)
            {
                others = ListComponent<CameraPluginRunner>.Create();
                for (int i = 0; i < Config.OtherPlugin.Length; i++)
                {
                    if(Config.OtherPlugin[i] == null) continue;
                    others.Add(RunnerFactory.CreateOtherPluginRunner(Config.OtherPlugin[i]));
                }
            }
        }
        
        public override void Update()
        {
            head?.Update();
            body.Update();
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    others[i]?.Update();
                }
            }
        }

        public override void Dispose()
        {
            body.Dispose();
            body = null;
            head.Dispose();
            head = null;
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    others[i].Dispose();
                }
                others.Dispose();
                others = null;
            }

            Data.Dispose();
            Data = null;
            
            ObjectPool.Instance.Recycle(this);
        }
    }
}