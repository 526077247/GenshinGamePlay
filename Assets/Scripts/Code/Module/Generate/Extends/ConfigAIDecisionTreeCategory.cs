using System;
using System.Collections.Generic;
using Unity.Code.NinoGen;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigAIDecisionTreeCategory: IManager
    {
        public static ConfigAIDecisionTreeCategory Instance { get; private set; }

        #region IManager

        private Dictionary<DecisionArchetype, ConfigAIDecisionTree> dict;
        private List<ConfigAIDecisionTree> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigAIDecisionTree>();
            dict = new Dictionary<DecisionArchetype, ConfigAIDecisionTree>();
            var sceneGroups = YooAssets.GetAssetInfos("aiTree");
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
                        var item = JsonHelper.FromJson<ConfigAIDecisionTree>(textAsset.text);
                        if (!dict.ContainsKey(item.Type))
                        {
                            _list.Add(item);
                            dict[item.Type] = item;
                        }
                        else
                        {
                            Log.Error("ConfigAIDecisionTree id重复 "+item.Type);
                        }
                    }
                    else if (Define.ConfigType == 1)
                    {
                        try
                        {
                            Deserializer.Deserialize(textAsset.bytes,out ConfigAIDecisionTree item);
                            if (!dict.ContainsKey(item.Type))
                            {
                                _list.Add(item);
                                dict[item.Type] = item;
                            }
                            else
                            {
                                Log.Error("ConfigAIDecisionTree id重复 "+item.Type);
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
            _list = null;
        }

        #endregion

        public ConfigAIDecisionTree Get(DecisionArchetype type)
        {
            this.dict.TryGetValue(type, out ConfigAIDecisionTree item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ConfigAIDecisionTree)}，配置id: {type}");
            }

            return item;
        }
        public Dictionary<DecisionArchetype, ConfigAIDecisionTree> GetAll()
        {
            return dict;
        }
        public List<ConfigAIDecisionTree> GetAllList()
        {
            return _list;
        }
    }
}