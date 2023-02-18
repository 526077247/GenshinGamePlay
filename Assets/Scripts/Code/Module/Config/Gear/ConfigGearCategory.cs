using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigGearCategory: IManager
    {
        public static ConfigGearCategory Instance { get; private set; }

        #region IManager

        private Dictionary<ulong, ConfigGear> _dict;
        private List<ConfigGear> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigGear>();
            _dict = new Dictionary<ulong, ConfigGear>();
            var gears = YooAssets.GetAssetInfos("gear");
            for (int i = 0; i < gears.Length; i++)
            {
                var op = YooAssets.LoadAssetSync(gears[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    var item = JsonHelper.FromJson<ConfigGear>(textAsset.text);
                    if (!_dict.ContainsKey(item.id))
                    {
                        _list.Add(item);
                        _dict[item.id] = item;
                    }
                }
                op.Release();
            }
        }

        public void Destroy()
        {
            Instance = null;
            _dict = null;
            _list = null;
        }

        #endregion

        public ConfigGear Get(ulong id)
        {
            return _dict[id];
        }
        public Dictionary<ulong, ConfigGear> GetAll()
        {
            return _dict;
        }
        public List<ConfigGear> GetAllList()
        {
            return _list;
        }
    }
}