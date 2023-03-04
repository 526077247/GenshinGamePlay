using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigAIDecisionTreeCategory: IManager
    {
        public static ConfigAIDecisionTreeCategory Instance { get; private set; }

        #region IManager

        private Dictionary<DecisionArchetype, ConfigAIDecisionTree> _dict;
        private List<ConfigAIDecisionTree> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigAIDecisionTree>();
            _dict = new Dictionary<DecisionArchetype, ConfigAIDecisionTree>();
            var sceneGroups = YooAssets.GetAssetInfos("aiTree");
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                var op = YooAssets.LoadAssetSync(sceneGroups[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    var item = JsonHelper.FromJson<ConfigAIDecisionTree>(textAsset.text);
                    if (!_dict.ContainsKey(item.Type))
                    {
                        _list.Add(item);
                        _dict[item.Type] = item;
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

        public ConfigAIDecisionTree Get(DecisionArchetype type)
        {
            _dict.TryGetValue(type, out var res);
            return res;
        }
        public Dictionary<DecisionArchetype, ConfigAIDecisionTree> GetAll()
        {
            return _dict;
        }
        public List<ConfigAIDecisionTree> GetAllList()
        {
            return _list;
        }
    }
}