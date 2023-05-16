namespace TaoTie
{
    public static class RunnerFactory
    {
        public static CameraBodyPluginRunner<T> CreateBodyPluginRunner<T>(T config)where T :ConfigCameraBodyPlugin
        {
            var res = ObjectPool.Instance.Fetch<CameraBodyPluginRunner<T>>();
            res.Init(config);
            return res;
        }
        
        public static CameraHeadPluginRunner<T> CreateHeadPluginRunner<T>(T config)where T :ConfigCameraHeadPlugin
        {
            var res = ObjectPool.Instance.Fetch<CameraHeadPluginRunner<T>>();
            res.Init(config);
            return res;
        }
        
        public static CameraOtherPluginRunner<T> CreateOtherPluginRunner<T>(T config)where T :ConfigCameraOtherPlugin
        {
            var res = ObjectPool.Instance.Fetch<CameraOtherPluginRunner<T>>();
            res.Init(config);
            return res;
        }
    }
}