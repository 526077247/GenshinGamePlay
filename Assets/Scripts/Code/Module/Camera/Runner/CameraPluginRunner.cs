namespace TaoTie
{
    public abstract class CameraPluginRunner
    {
        
    }
    public abstract class CameraPluginRunner<T>: CameraPluginRunner where T: ConfigCameraPlugin
    {
        protected T plugin;

        public void Init(T config)
        {
            plugin = config;
        }
    }
}