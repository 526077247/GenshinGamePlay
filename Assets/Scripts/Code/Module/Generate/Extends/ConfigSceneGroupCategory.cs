using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigSceneGroupCategory: IManager
    {
        public static ConfigSceneGroupCategory Instance { get; private set; }

        #region IManager

        private Dictionary<ulong, ConfigSceneGroup> dict;
        private List<ConfigSceneGroup> list;

        public void Init()
        {
            Instance = this;
            list = new List<ConfigSceneGroup>();
            dict = new Dictionary<ulong, ConfigSceneGroup>();
            var sceneGroups = YooAssets.GetAssetInfos("sceneGroup");
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                var op = YooAssets.LoadAssetSync(sceneGroups[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    var item = JsonHelper.FromJson<ConfigSceneGroup>(textAsset.text);
                    if (!dict.ContainsKey(item.Id))
                    {
                        list.Add(item);
                        dict[item.Id] = item;
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
            dict = null;
            list = null;
        }

        #endregion

        public ConfigSceneGroup Get(ulong id)
        {
            this.dict.TryGetValue(id, out ConfigSceneGroup item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ConfigSceneGroup)}，配置id: {id}");
            }

            return item;
        }
        public Dictionary<ulong, ConfigSceneGroup> GetAll()
        {
            return dict;
        }
        public List<ConfigSceneGroup> GetAllList()
        {
            return list;
        }
    }
}