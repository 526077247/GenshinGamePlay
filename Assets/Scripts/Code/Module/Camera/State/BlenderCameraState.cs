namespace TaoTie
{
    public class BlenderCameraState: CameraState
    {
        public override bool IsBlenderState => true;
        private ConfigCameraBlender config;

        private NormalCameraState from;
        private NormalCameraState to;

        private CameraStateData formData;
        private CameraStateData toData;

        private EasingFunction.Function lerpFunc;

        private long startlerpTime;
        
        public static BlenderCameraState Create(NormalCameraState from, NormalCameraState to,bool isEnter)
        {
            BlenderCameraState res = ObjectPool.Instance.Fetch<BlenderCameraState>();
            res.from = from;
            res.to = to;
            res.Data = CameraStateData.DeepClone(res.from.Data);
            if (isEnter)
            {
                res.config = to.Config.Enter;
            }
            else
            {
                res.config = from.Config.Level;
            }

            res.lerpFunc = EasingFunction.GetEasingFunction(res.config.Ease);
            res.startlerpTime = GameTimerManager.Instance.GetTimeNow();
            return res;
        }
        
        public override void Update()
        {
            
        }

        /// <summary>
        /// 切换过渡目标
        /// </summary>
        /// <param name="to"></param>
        /// <param name="isEnter"></param>
        public void ChangeTo(NormalCameraState to,bool isEnter)
        {
            this.to = to;
            formData = Data;
            Data = CameraStateData.DeepClone(formData);
            toData = to.Data;
            if (isEnter)
            {
                config = to.Config.Enter;
            }
            else
            {
                config = from.Config.Level;
            }

            lerpFunc = EasingFunction.GetEasingFunction(config.Ease);
            startlerpTime = GameTimerManager.Instance.GetTimeNow();
        }

        public override void Dispose()
        {
            Data.Dispose();
            Data = null;
            from = null;
            to = null;
            config = null;
            lerpFunc = null;
            startlerpTime = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}