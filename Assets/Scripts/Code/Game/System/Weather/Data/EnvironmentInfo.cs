using System;

namespace TaoTie
{
    public class EnvironmentInfo: IDisposable
    {
        public bool Changed;
        public bool IsBlender;
        
        public static EnvironmentInfo Create(EnvironmentConfig config)
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
        public void Lerp(EnvironmentConfig from, EnvironmentConfig to, float val)
        {
            
        }
        public void Dispose()
        {
            
        }
    }
}