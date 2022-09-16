using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ServerConfigManager:IManager
    {
        public readonly string ServerKey = "ServerId";
        public readonly int defaultServer = 1;
        public ServerConfig cur_config;
        public static ServerConfigManager Instance;
		
        public string m_update_list_cdn_url;
        public string m_cdn_url;
        public bool m_inWhiteList;
        public Dictionary<string, Dictionary<int, Resver>> m_resUpdateList;
        public Dictionary<string, AppConfig> m_appUpdateList;
        #region override

        public void Init()
        {
            Instance = this;
            if(Define.Debug)
                this.cur_config = ServerConfigCategory.Instance.Get(PlayerPrefs.GetInt(this.ServerKey, this.defaultServer));
            if (this.cur_config == null)
            {
                foreach (var item in ServerConfigCategory.Instance.GetAll())
                {
                    this.cur_config = item.Value;
                    if (item.Value.IsPriority==1)
                        break;
                }
            }

            this.m_update_list_cdn_url = this.cur_config.UpdateListUrl;
            this.m_cdn_url = this.cur_config.ResUrl;
        }

        public void Destroy()
        {
            Instance = null;
            m_resUpdateList.Clear();
            m_appUpdateList.Clear();
        }

        #endregion
        
        public ServerConfig GetCurConfig()
        {
            return this.cur_config;

        }

        public ServerConfig ChangeEnv(int id)
        {
            var conf = ServerConfigCategory.Instance.Get(id);
            if(conf!=null)
            {
                this.cur_config = conf;
                if (Define.Debug)
                    PlayerPrefs.SetInt(this.ServerKey, id);
            }
            return this.cur_config;

        }
        
        //获取测试环境更新列表cdn地址
        public string GetTestUpdateListCdnUrl()
        {
            return this.cur_config.TestUpdateListUrl;
        }

        public int GetEnvId()
        {
            return this.cur_config.EnvId;
        }
        
        #region 白名单
        //获取白名单下载地址
        public string GetWhiteListCdnUrl()
        {
            if (string.IsNullOrEmpty(this.m_update_list_cdn_url)) return this.m_update_list_cdn_url;
            return string.Format("{0}/white.list", this.m_update_list_cdn_url);
        }

        //设置白名单模式
        public void SetWhiteMode(bool whiteMode)
        {
            if (whiteMode)
            {
                this.m_update_list_cdn_url = this.GetTestUpdateListCdnUrl();
            }
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
            this.m_inWhiteList = false;
            var env_id = this.GetEnvId();
            var account = PlayerPrefs.GetString(CacheKeys.Account);
            foreach (var item in info)
            {
                if (item.env_id == env_id && item.account == account)
                {
                    this.m_inWhiteList = true;
                    Log.Info(" user is in white list "+account);
                    break;
                }
            }
        }
        //是否在白名单中
        public bool IsInWhiteList()
        {
            return this.m_inWhiteList;
        }
        #endregion

        //获取更新列表地址, 平台独立
        //格式为json格式
        //    {
        //        "res_list" : {
        //                "googleplay": {
        //                       "1.0.0": {"channel": ["all"], "update_tailnumber": ["all"]},
        //                 }
        //        },
        //        "app_list" : { 
        //                 "googleplay": {
        //                      "app_url": "https://www.baidu.com/",
        //                       "app_ver": {
        //	                           "1.0.1": { "force_update": 1 }
        //                       }
        //                  }
        //         }
        //    }
        public string GetUpdateListCdnUrl()
        {
            var url = string.Format("{0}/update_{1}.list", this.m_update_list_cdn_url, PlatformUtil.GetStrPlatformIgnoreEditor());
            Log.Info("GetUpdateListUrl url = "+url);
            return url;
        }

        //设置更新列表
        public void SetUpdateList(UpdateConfig info)
        {
            this.m_appUpdateList = info.app_list;
            this.m_resUpdateList = info.res_list;
        }

        //根据渠道获取app更新列表
        public  AppConfig GetAppUpdateListByChannel(string channel)
        {
            if (this.m_appUpdateList == null) return null;
            if(this.m_appUpdateList.TryGetValue(channel,out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = this.m_appUpdateList[data.jump_channel];
                return data;
            }
            return null;
        }
        //找到可以更新的最大app版本号
        public int FindMaxUpdateAppVer(string channel)
        {
            if (this.m_appUpdateList == null) return -1;
            int last_ver = -1;
            if (this.m_appUpdateList.TryGetValue(channel, out var data))
            {
                foreach (var item in data.app_ver)
                {
                    if (last_ver == -1) last_ver = item.Key;
                    else
                    {
                        if(item.Key > last_ver)
                        {
                            last_ver = item.Key;
                        }
                    }
                }
            }
            return last_ver;
        }

        //找到可以更新的最大资源版本号
        public int FindMaxUpdateResVer(string appchannel, string channel,out Resver resver)
        {
            resver = null;
            if (string.IsNullOrEmpty(appchannel) || this.m_resUpdateList == null || 
                !this.m_resUpdateList.TryGetValue(appchannel, out var resVerList)) return -1;
            if (resVerList == null) return -1;
            var verList = new List<int>();
            foreach (var item in resVerList)
            {
                verList.Add(item.Key);
            }
            verList.Sort((a, b) => { return b-a; });
            int last_ver = -1;
            for (int i = 0; i < verList.Count; i++)
            {
                var info = resVerList[verList[i]];
                if(this.IsStrInList(channel,info.channel)&& this.IsInTailNumber(info.update_tailnumber))
                {
                    last_ver = verList[i];
                    break;
                }
            }
            resver = resVerList[last_ver];
            return last_ver;
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

        //根据资源版本号获取在cdn上的资源地址
        public string GetUpdateCdnResUrlByVersion(string resver)
        {
            var platformStr = PlatformUtil.GetStrPlatformIgnoreEditor();
            var url = string.Format("{0}/{1}_{2}", this.m_cdn_url, resver, platformStr);
            Log.Info("GetUpdateCdnResUrl url = "+url);
            return url;
        }
    }
}