using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class PackageConfig
    {
        public int DefaultPackageVersion;
        public Dictionary<int, string[]> OtherPackageMaxVer;

        public int GetPackageMaxVersion(string name)
        {
            if (name == Define.DefaultName)
            {
                return DefaultPackageVersion;
            }
            var ver = -1;
            if (OtherPackageMaxVer == null) return ver;
            foreach (var item in OtherPackageMaxVer)
            {
                if(item.Value == null) continue;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (item.Value[i] == name)
                    {
                        return item.Key;
                    } 
                }
            }
            return ver;
        }
    }
    
    public class WhiteConfig
    {
        public int EnvId;
        public string Account;
    }
    
    public class Resver
    {
        public List<string> Channel;
        public List<string> UpdateTailNumber;
        public int ForceUpdate;
        public int MaxResVer;
    }
    public class AppConfig
    {
        public string AppUrl;
        public Dictionary<int, Resver> AppVer;
        public string JumpChannel;
    }
    public class UpdateConfig
    {
        public Dictionary<string,Dictionary<int, Resver>> ResList;
        public Dictionary<string, AppConfig> AppList;
    }
}