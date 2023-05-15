using System.Collections.Generic;

namespace TaoTie
{
    public class NormalCameraState: CameraState
    {

        private ConfigCamera config;
        
        private CameraPluginRunner body;
        private CameraPluginRunner head;
        private List<CameraPluginRunner> others;

        public void Init(ConfigCamera config)
        {
            this.config = config;
        }
        
        public override void Update()
        {
            
        }
    }
}