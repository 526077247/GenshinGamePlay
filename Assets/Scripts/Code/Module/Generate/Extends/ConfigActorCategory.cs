using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ConfigActorCategory : IManager
    {
        public static ConfigActorCategory Instance { get; private set; }

        #region IManager

        private Dictionary<string, ConfigActor> dict;
        private List<ConfigActor> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigActor>();
            dict = new Dictionary<string, ConfigActor>();

        }

        public async ETTask LoadAsync()
        {
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                var list = UnitConfigCategory.Instance.GetAllList();
                for (int i = 0; i < list.Count; i++)
                {
                    string path = list[i].ActorConfig;
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
                dict[path] = JsonHelper.FromJson<ConfigActor>(jStr);
                return;
            }
#if RoslynAnalyzer
            else
            {
                var bytes = await ResourcesManager.Instance.LoadConfigBytesAsync(path);
                Deserializer.Deserialize(bytes,out ConfigActor res);
                dict[path] =  res;
                return;
            }
#endif
            Log.Error($"GetConfigActor 失败，ConfigType = {Define.ConfigType} 未处理");
        }

        public void Destroy()
        {
            Instance = null;
            dict = null;
            _list = null;
        }

        #endregion

        public ConfigActor Get(string path)
        {
            this.dict.TryGetValue(path, out ConfigActor item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof(ConfigActor)}，配置id: {path}");
            }

            return item;
        }

        public Dictionary<string, ConfigActor> GetAll()
        {
            return dict;
        }

        public List<ConfigActor> GetAllList()
        {
            return _list;
        }
    }
}