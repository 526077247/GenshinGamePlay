using System;

namespace TaoTie
{
    public class EnvironmentInfo: IDisposable
    {
        public bool Changed;
        public bool IsBlender;
        
        public static EnvironmentInfo Create(ConfigEnvironment config)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();

            return res;
        }

        public static EnvironmentInfo DeepClone(EnvironmentInfo other)
        {
            EnvironmentInfo res = ObjectPool.Instance.Fetch<EnvironmentInfo>();

            return res;
        }

        public void Lerp(EnvironmentInfo from, EnvironmentInfo to, float val)
        {
            
        }
        public void Lerp(ConfigEnvironment from, ConfigEnvironment to, float val)
        {
            
        }
        public void Dispose()
        {
            
        }
    }
}