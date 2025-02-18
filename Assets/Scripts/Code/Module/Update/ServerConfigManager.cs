using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ServerConfigManager:IManager
    {
        private readonly string serverKey = "ServerId";
        private readonly int defaultServer = 1;
        private ServerConfig curConfig;
        public static ServerConfigManager Instance;
        
        private bool inWhiteList;
        private Dictionary<string, Dictionary<int, Resver>> resUpdateList;
        private Dictionary<string, AppConfig> appUpdateList;
        #region override

        public void Init()
        {
            Instance = this;
            if(Define.Debug)
                this.curConfig = ServerConfigCategory.Instance.Get(CacheManager.Instance.GetInt(this.serverKey, this.defaultServer));
            if (this.curConfig == null)
            {
                foreach (var item in ServerConfigCategory.Instance.GetAll())
                {
                    this.curConfig = item.Value;
                    if (item.Value.IsPriority==1)
                        break;
                }
            }
        }

        public void Destroy()
        {
            Instance = null;
            resUpdateList = null;
            appUpdateList = null;
        }

        #endregion
        
        public ServerConfig GetCurConfig()
        {
            return this.curConfig;
        }

        public ServerConfig ChangeEnv(int id)
        {
            var conf = ServerConfigCategory.Instance.Get(id);
            if(conf!=null)
            {
                this.curConfig = conf;
                if (Define.Debug)
                    CacheManager.Instance.SetInt(this.serverKey, id);
            }
            return this.curConfig;

        }
        
        //获取环境更新列表cdn地址
        public string GetUpdateListUrl()
        {
            return RemoteServices.Instance.whiteMode? PackageManager.Instance.CdnConfig.TestUpdateListUrl:PackageManager.Instance.CdnConfig.UpdateListUrl;
        }

        public int GetEnvId()
        {
            return this.curConfig.EnvId;
        }
        
        #region 白名单
        //获取白名单下载地址
        public string GetWhiteListCdnUrl()
        {
            var url = GetUpdateListUrl();
            if (string.IsNullOrEmpty(url)) return url;
            return string.Format("{0}/white.list?timestamp={1}", url, TimerManager.Instance.GetTimeNow());
        }

        //设置白名单模式
        public void SetWhiteMode(bool whiteMode)
        {
            RemoteServices.Instance.whiteMode = whiteMode;
        }

        //设置白名单列表
        //格式为json格式
	    //{
		// "WhiteList":[
    	//	    {"EnvId":1, "Account":11111}
    	//    ]
        //}
        public void SetWhiteList(List<WhiteConfig> info)
        {
            this.inWhiteList = false;
            var envID = this.GetEnvId();
            var account = CacheManager.Instance.GetString(CacheKeys.Account);
            foreach (var item in info)
            {
                if (item.EnvId == envID && item.Account == account)
                {
                    this.inWhiteList = true;
                    Log.Info(" user is in white list "+account);
                    break;
                }
            }
        }
        //是否在白名单中
        public bool IsInWhiteList()
        {
            return this.inWhiteList;
        }
        #endregion

        //获取更新列表地址, 平台独立
        public string GetUpdateListCdnUrl()
        {
            var url = string.Format("{0}/update_{1}.list?timestamp={2}", this.GetUpdateListUrl(), 
                PlatformUtil.GetStrPlatformIgnoreEditor(),TimerManager.Instance.GetTimeNow());
            Log.Info("GetUpdateListUrl url = "+url);
            return url;
        }

        //设置更新列表
        public void SetUpdateList(UpdateConfig info)
        {
            this.appUpdateList = info.AppList;
            this.resUpdateList = info.ResList;
        }

        //根据渠道获取app更新列表
        public AppConfig GetAppUpdateListByChannel(string channel)
        {
            if (this.appUpdateList == null) return null;
            if (this.appUpdateList.TryGetValue(channel, out var data))
            {
                if (GetJumpChannel(data.JumpChannel,out var jumpData))
                {
                    var newData = new AppConfig();
                    newData.AppVer = jumpData.AppVer;
                    newData.AppUrl = data.AppUrl;
                    return newData;
                }

                return data;
            }

            return null;
        }

        private bool GetJumpChannel(string jumpChannel,out AppConfig jumpData)
        {
            if (!string.IsNullOrEmpty(jumpChannel) && this.appUpdateList.TryGetValue(jumpChannel, out jumpData))
            {
                if(GetJumpChannel(jumpData.JumpChannel,out var newdata))
                {
                    jumpData = newdata;
                }
                return true;
            }
            jumpData = null;
            return false;
        }

        //找到可以更新的最大app版本号
        public int FindMaxUpdateAppVer(string channel)
        {
            if (this.appUpdateList == null) return -1;
            int lastVer = -1;
            if (this.appUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.JumpChannel))
                    data = appUpdateList[data.JumpChannel];
                foreach (var item in data.AppVer)
                {
                    if (lastVer == -1) lastVer = item.Key;
                    else
                    {
                        if(item.Key > lastVer
                           && IsStrInList(channel,item.Value.Channel) && IsInTailNumber(item.Value.UpdateTailNumber))
                        {
                            lastVer = item.Key;
                        }
                    }
                }
            }
            return lastVer;
        }
        //找到可以更新的最大app版本号
        public bool FindMaxUpdateResVerThisAppVer(string channel,int appVer,out int version)
        {
            version = -1;
            if (this.appUpdateList == null) return false;
            if (this.appUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.JumpChannel))
                    data = appUpdateList[data.JumpChannel];
                if (data.AppVer.TryGetValue(appVer, out var res))
                {
                    version = res.MaxResVer;
                    return true;
                }
            }
            return false;
        }
        //找到可以更新的最大资源版本号
        public int FindMaxUpdateResVer(string configChannel, string resverChannel,int appResVer)
        {
            var rename = "common";
            for (int i = 0; i < Define.RenameList.Length; i++)
            {
                if (Define.RenameList[i] == configChannel)
                {
                    rename = configChannel;
                    break;
                }
            }

            configChannel = rename;
            
            if (string.IsNullOrEmpty(configChannel) || this.resUpdateList == null || 
                !this.resUpdateList.TryGetValue(configChannel, out var resVerList)) return -1;
            if (resVerList == null) return -1;
            var verList = new List<int>();
            foreach (var item in resVerList)
            {
                verList.Add(item.Key);
            }
            verList.Sort((a, b) => { return b-a; });
            int lastVer = -1;
            for (int i = 0; i < verList.Count; i++)
            {
                var info = resVerList[verList[i]];
                if(this.IsStrInList(resverChannel,info.Channel)&& this.IsInTailNumber(info.UpdateTailNumber))
                {
                    lastVer = verList[i];
                    break;
                }
            }

            if (appResVer>0 && lastVer > appResVer&&resVerList.ContainsKey(appResVer))
            {
                return appResVer;
            }
            return lastVer;
        }

        public Resver GetResVerInfo(string configChannel, int version)
        {
            var rename = "common";
            for (int i = 0; i < Define.RenameList.Length; i++)
            {
                if (Define.RenameList[i] == configChannel)
                {
                    rename = configChannel;
                    break;
                }
            }

            configChannel = rename;
            if (string.IsNullOrEmpty(configChannel) || this.resUpdateList == null || 
                !this.resUpdateList.TryGetValue(configChannel, out var resVerList)) return null;
            if (resVerList.TryGetValue(version, out var res))
            {
                return res;
            }

            return null;
        }
        //检测灰度更新，检测是否在更新尾号列表
        public bool IsInTailNumber(List<string> list)
        {
            if (list == null) return false;
            var account = CacheManager.Instance.GetString(CacheKeys.Account, "");
            var tailNumber = "";
            if (!string.IsNullOrEmpty(account))
                tailNumber = account[account.Length - 1].ToString();
            for (int i = 0; i < list.Count; i++)
                if (list[i] == "all" || tailNumber == list[i])
                    return true;
            return false;
        }

        public bool IsStrInList(string str,List<string> list)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == "all" || str == list[i])
                    return true;
            return false;
        }
    }
}