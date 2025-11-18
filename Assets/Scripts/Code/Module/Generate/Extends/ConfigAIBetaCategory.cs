using System;
using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    public class ConfigAIBetaCategory : IManager
    {
        public static ConfigAIBetaCategory Instance { get; private set; }

        #region IManager

        private Dictionary<string, ConfigAIBeta> dict;
        private List<ConfigAIBeta> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigAIBeta>();
            dict = new Dictionary<string, ConfigAIBeta>();

        }

        public async ETTask LoadAsync()
        {
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                var list = MonsterConfigCategory.Instance.GetAllList();
                for (int i = 0; i < list.Count; i++)
                {
                    string path = list[i].AIPath;
                    if (!string.IsNullOrEmpty(path) && !dict.ContainsKey(path))
                    {
                        dict.Add(path,null);
                        tasks.Add(LoadOneAsync(path));
                    }
                }
                var list2 = GadgetConfigCategory.Instance.GetAllList();
                for (int i = 0; i < list2.Count; i++)
                {
                    string path = list2[i].AIPath;
                    if (!string.IsNullOrEmpty(path) && !dict.ContainsKey(path))
                    {
                        dict.Add(path,null);
                        tasks.Add(LoadOneAsync(path));
                    }
                }
                await ETTaskHelper.WaitAll(tasks);
            }
        }

        private async ETTask LoadOneAsync(string path)
        {
            if (Define.ConfigType == 0)
            {
                var jStr = await ResourcesManager.Instance.LoadConfigJsonAsync(path);
                dict[path] = JsonHelper.FromJson<ConfigAIBeta>(jStr);
            }
            else
            {
                var bytes = await ResourcesManager.Instance.LoadConfigBytesAsync(path);
                dict[path] = NinoDeserializer.Deserialize<ConfigAIBeta>(bytes);
            }
        }

        public void Destroy()
        {
            Instance = null;
            dict = null;
            _list = null;
        }

        #endregion

        public ConfigAIBeta Get(string path)
        {
            this.dict.TryGetValue(path, out ConfigAIBeta item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof(ConfigAIBeta)}，配置id: {path}");
            }

            return item;
        }

        public Dictionary<string, ConfigAIBeta> GetAll()
        {
            return dict;
        }

        public List<ConfigAIBeta> GetAllList()
        {
            return _list;
        }
    }
}