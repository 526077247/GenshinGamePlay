using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    [Serializable][CreateAssetMenu(menuName = "CDNConfig")]
    public class CDNConfig:ScriptableObject
    {
        public string Channel;
        public string DefaultHostServer;
        public string FallbackHostServer;
    }
}