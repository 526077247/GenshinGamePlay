﻿using System.Collections.Generic;
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
        private bool whiteMode = false;
        private Dictionary<string, Dictionary<int, Resver>> resUpdateList;
        private Dictionary<string, AppConfig> appUpdateList;
        #region override

        public void Init()
        {
            Instance = this;
            if(Define.Debug)
                this.curConfig = ServerConfigCategory.Instance.Get(PlayerPrefs.GetInt(this.serverKey, this.defaultServer));
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
                    PlayerPrefs.SetInt(this.serverKey, id);
            }
            return this.curConfig;

        }
        
        //获取环境更新列表cdn地址
        public string GetUpdateListUrl()
        {
            return this.whiteMode? YooAssetsMgr.Instance.CdnConfig.TestUpdateListUrl:YooAssetsMgr.Instance.CdnConfig.UpdateListUrl;
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
            return string.Format("{0}/white.list", url);
        }

        //设置白名单模式
        public void SetWhiteMode(bool whiteMode)
        {
            this.whiteMode = whiteMode;
        }

        //设置白名单列表
        //格式为json格式
	    //{
		// "WhiteList":[
    	//	    {"env_id":1, "uid":11111}
    	//    ]
        //}
        public void SetWhiteList(List<WhiteConfig> info)
        {
            this.inWhiteList = false;
            var env_id = this.GetEnvId();
            var account = PlayerPrefs.GetString(CacheKeys.Account);
            foreach (var item in info)
            {
                if (item.env_id == env_id && item.account == account)
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
            this.appUpdateList = info.app_list;
            this.resUpdateList = info.res_list;
        }

        //根据渠道获取app更新列表
        public  AppConfig GetAppUpdateListByChannel(string channel)
        {
            if (this.appUpdateList == null) return null;
            if(this.appUpdateList.TryGetValue(channel,out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = this.appUpdateList[data.jump_channel];
                return data;
            }
            return null;
        }
        //找到可以更新的最大app版本号
        public int FindMaxUpdateAppVer(string channel)
        {
            if (this.appUpdateList == null) return -1;
            int last_ver = -1;
            if (this.appUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = appUpdateList[data.jump_channel];
                foreach (var item in data.app_ver)
                {
                    if (last_ver == -1) last_ver = item.Key;
                    else
                    {
                        if(item.Key > last_ver
                           && IsStrInList(channel,item.Value.channel) && IsInTailNumber(item.Value.update_tailnumber))
                        {
                            last_ver = item.Key;
                        }
                    }
                }
            }
            return last_ver;
        }
        //找到可以更新的最大app版本号
        public bool FindMaxUpdateResVerThisAppVer(string channel,int appVer,out int version)
        {
            version = -1;
            if (this.appUpdateList == null) return false;
            if (this.appUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = appUpdateList[data.jump_channel];
                if (data.app_ver.TryGetValue(appVer, out var res))
                {
                    version = res.max_res_ver;
                    return true;
                }
            }
            return false;
        }
        //找到可以更新的最大资源版本号
        public int FindMaxUpdateResVer(string configChannel, string resverChannel,int appResVer, out Resver resver)
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
            resver = null;
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
                if(this.IsStrInList(resverChannel,info.channel)&& this.IsInTailNumber(info.update_tailnumber))
                {
                    lastVer = verList[i];
                    break;
                }
            }

            if (appResVer>0 && lastVer > appResVer&&resVerList.TryGetValue(appResVer,out resver))
            {
                return appResVer;
            }
            resver = resVerList[lastVer];
            return lastVer;
        }
        //检测灰度更新，检测是否在更新尾号列表
        public bool IsInTailNumber(List<string> list)
        {
            if (list == null) return false;
            var account = PlayerPrefs.GetString(CacheKeys.Account, "");
            var tail_number = "";
            if (!string.IsNullOrEmpty(account))
                tail_number = account[account.Length - 1].ToString();
            for (int i = 0; i < list.Count; i++)
                if (list[i] == "all" || tail_number == list[i])
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