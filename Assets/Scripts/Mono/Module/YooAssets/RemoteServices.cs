using System;
using TaoTie;
using YooAsset;

namespace YooAsset
{
    public class RemoteServices: IRemoteServices
    {
        public static RemoteServices Instance { get; private set; }
        public bool whiteMode = false;
        private CDNConfig conf;
        private string rename;
        public RemoteServices(CDNConfig config)
        {
            conf = config;
            Instance = this;
            rename = conf.GetChannel();
        }
        public string GetRemoteMainURL(string fileName)
        {
            return $"{(whiteMode?conf.TestUpdateListUrl:conf.DefaultHostServer)}/{rename}_{PlatformUtil.GetStrPlatformIgnoreEditor()}/{fileName}";
        }

        public string GetRemoteFallbackURL(string fileName)
        {
            return $"{(whiteMode?conf.TestUpdateListUrl:conf.FallbackHostServer)}/{rename}_{PlatformUtil.GetStrPlatformIgnoreEditor()}/{fileName}";
        }
    }
}