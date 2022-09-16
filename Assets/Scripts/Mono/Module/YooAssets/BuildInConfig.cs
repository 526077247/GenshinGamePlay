using System.Collections.Generic;

namespace TaoTie
{
    public class BuildInConfig
    {
        public string Channel;
        public int Resver;
        public string RemoteCdnUrl;
        public string RemoteCdnUrl2;
    }
    
    public class WhiteConfig
    {
        public int env_id;
        public string account;
    }
    
    public class Resver
    {
        public List<string> channel;
        public List<string> update_tailnumber;
        public int force_update;
    }
    public class AppConfig
    {
        public string app_url;
        public Dictionary<int, Resver> app_ver;
        public string jump_channel;
    }
    public class UpdateConfig
    {
        public Dictionary<string,Dictionary<int, Resver>> res_list;
        public Dictionary<string, AppConfig> app_list;
    }
}