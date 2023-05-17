namespace TaoTie
{
    public class BlenderCameraState: CameraState
    {
        public override bool IsBlenderState => true;
        private ConfigCameraBlender config;

        public NormalCameraState From { get; private set; }
        public NormalCameraState To { get; private set; }

        private CameraStateData formData;
        private CameraStateData toData;

        private EasingFunction.Function lerpFunc;

        private long startlerpTime;
        
        public static BlenderCameraState Create(NormalCameraState from, NormalCameraState to,bool isEnter)
        {
            BlenderCameraState res = ObjectPool.Instance.Fetch<BlenderCameraState>();
            res.Id = IdGenerater.Instance.GenerateId();
            res.From = from;
            res.To = to;
            res.Data = CameraStateData.DeepClone(res.From.Data);
            res.Priority = to.Priority;
            if (isEnter)
            {
                res.config = to.Config.Enter;
            }
            else
            {
                res.config = from.Config.Level;
            }

            res.IsOver = false;
            res.lerpFunc = EasingFunction.GetEasingFunction(res.config.Ease);
            res.startlerpTime = GameTimerManager.Instance.GetTimeNow();
            return res;
        }

        public override void Update()
        {
            if (To.IsOver) IsOver = true;
            if (IsOver) return;
            if (GameTimerManager.Instance.GetTimeNow() > startlerpTime + config.DeltaTime)
            {
                IsOver = true;
            }
        }

        /// <summary>
        /// 切换过渡目标
        /// </summary>
        /// <param name="to"></param>
        /// <param name="isEnter"></param>
        public void ChangeTo(NormalCameraState to,bool isEnter)
        {
            this.To = to;
            formData = Data;
            Data = CameraStateData.DeepClone(formData);
            toData = to.Data;
            if (isEnter)
            {
                config = to.Config.Enter;
            }
            else
            {
                config = From.Config.Level;
            }
            Priority = to.Priority;
            IsOver = false;
            lerpFunc = EasingFunction.GetEasingFunction(config.Ease);
            startlerpTime = GameTimerManager.Instance.GetTimeNow();
        }

        public override void Dispose()
        {
            if (From.IsOver)
            {
                From.Dispose();
            }
            Data.Dispose();
            Data = null;
            From = null;
            To = null;
            config = null;
            lerpFunc = null;
            startlerpTime = default;
            IsOver = true;
            ObjectPool.Instance.Recycle(this);
        }
    }
}