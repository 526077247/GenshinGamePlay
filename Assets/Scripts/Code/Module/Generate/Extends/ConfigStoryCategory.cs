using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigStoryCategory: IManager
    {
        public static ConfigStoryCategory Instance { get; private set; }

        #region IManager

        private Dictionary<ulong, ConfigStory> dict;
        private List<ConfigStory> list;

        public void Init()
        {
            Instance = this;
            list = new List<ConfigStory>();
            dict = new Dictionary<ulong, ConfigStory>();
            var sceneGroups = YooAssets.GetAssetInfos("story");
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                var op = YooAssets.LoadAssetSync(sceneGroups[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    var item = JsonHelper.FromJson<ConfigStory>(textAsset.text);
                    if (!dict.ContainsKey(item.Id))
                    {
                        list.Add(item);
                        dict[item.Id] = item;
                    }
                    else
                    {
                        Log.Error("ConfigStory id重复 "+item.Id);
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

        public ConfigStory Get(ulong id)
        {
            this.dict.TryGetValue(id, out ConfigStory item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ConfigStory)}，配置id: {id}");
            }

            return item;
        }
        public Dictionary<ulong, ConfigStory> GetAll()
        {
            return dict;
        }
        public List<ConfigStory> GetAllList()
        {
            return list;
        }
    }
}