using System;
using System.Collections.Generic;
using Unity.Code.NinoGen;
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
                if (sceneGroups[i].AssetPath.EndsWith("json") && Define.ConfigType != 0)
                {
                    continue;
                }
                if (sceneGroups[i].AssetPath.EndsWith("bytes") && Define.ConfigType != 1)
                {
                    continue;
                }
                var op = YooAssets.LoadAssetSync(sceneGroups[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    if (Define.ConfigType == 0)
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
                    else if (Define.ConfigType == 1)
                    {
                        try
                        {
                            Deserializer.Deserialize(textAsset.bytes,out ConfigStory item);
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
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
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