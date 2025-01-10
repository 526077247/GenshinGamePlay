using System;

namespace TaoTie
{
    public class MsgBoxInfoNode : IDisposable
    {
        public Type Type;
        public MsgBoxPara Para;
        public string Path;
        public UILayerNames Layer;

        public static MsgBoxInfoNode Create()
        {
            return ObjectPool.Instance.Fetch<MsgBoxInfoNode>();
        }

        public void Dispose()
        {
            Para = null;
            Type = null;
            Path = null;
            ObjectPool.Instance.Recycle(this);
        }

    }
}