using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigSceneGroupCategory: IManager
    {
        public static ConfigSceneGroupCategory Instance { get; private set; }

        #region IManager

        private Dictionary<ulong, ConfigSceneGroup> _dict;
        private List<ConfigSceneGroup> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigSceneGroup>();
            _dict = new Dictionary<ulong, ConfigSceneGroup>();
            var sceneGroups = YooAssets.GetAssetInfos("sceneGroup");
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                var op = YooAssets.LoadAssetSync(sceneGroups[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    var item = JsonHelper.FromJson<ConfigSceneGroup>(textAsset.text);
                    if (!_dict.ContainsKey(item.Id))
                    {
                        _list.Add(item);
                        _dict[item.Id] = item;
                    }
                    else
                    {
                        Log.Error("ConfigSceneGroup id重复 "+item.Id);
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

        public ConfigSceneGroup Get(ulong id)
        {
            return _dict[id];
        }
        public Dictionary<ulong, ConfigSceneGroup> GetAll()
        {
            return _dict;
        }
        public List<ConfigSceneGroup> GetAllList()
        {
            return _list;
        }
    }
}