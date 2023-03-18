using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TaoTie
{
    public class ConfigAbilityCategory: IManager
    {
        public static ConfigAbilityCategory Instance { get; private set; }

        #region IManager

        private Dictionary<string, ConfigAbility> _dict;
        private List<ConfigAbility> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigAbility>();
            _dict = new Dictionary<string, ConfigAbility>();
            var sceneGroups = YooAssets.GetAssetInfos("ability");
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                var op = YooAssets.LoadAssetSync(sceneGroups[i]);
                if (op.AssetObject is TextAsset textAsset)
                {
                    if (JsonHelper.TryFromJson<List<ConfigAbility>>(textAsset.text, out var list))
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            var item = list[j];
                            if (!_dict.ContainsKey(item.AbilityName))
                            {
                                _list.Add(item);
                                _dict[item.AbilityName] = item;
                            }
                        }
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

        public ConfigAbility Get(string name)
        {
            return _dict[name];
        }
        public Dictionary<string, ConfigAbility> GetAll()
        {
            return _dict;
        }
        public List<ConfigAbility> GetAllList()
        {
            return _list;
        }
        
        public ListComponent<ConfigAbility> GetList(string[] names)
        {
            ListComponent<ConfigAbility> res = ListComponent<ConfigAbility>.Create();
            if (names != null)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    res.Add(Get(names[i]));
                }
            }

            return res;
        }
    }
}