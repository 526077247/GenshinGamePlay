using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class NavmeshSystem:IManager
    {
        public static NavmeshSystem Instance;
        private readonly Dictionary<string, byte[]> navmeshs = new();
        
        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
        }
        
        public byte[] Get(string name)
        {
            lock (this)
            {
                if (this.navmeshs.TryGetValue(name, out byte[] bytes))
                {
                    return bytes;
                }

                byte[] buffer = ResourcesManager.Instance.Load<TextAsset>(name).bytes;
                if (buffer.Length == 0)
                {
                    throw new Exception($"no nav data: {name}");
                }

                this.navmeshs[name] = buffer;
                return buffer;
            }
        }
    }
}