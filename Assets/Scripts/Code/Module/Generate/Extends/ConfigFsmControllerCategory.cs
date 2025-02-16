using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ConfigFsmControllerCategory : IManager
    {
        public static ConfigFsmControllerCategory Instance { get; private set; }

        #region IManager

        private Dictionary<string, ConfigFsmController> dict;
        private List<ConfigFsmController> _list;

        public void Init()
        {
            Instance = this;
            _list = new List<ConfigFsmController>();
            dict = new Dictionary<string, ConfigFsmController>();

        }

        public async ETTask LoadAsync()
        {
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                var list = UnitConfigCategory.Instance.GetAllList();
                for (int i = 0; i < list.Count; i++)
                {
                    string path = list[i].FSM;
                    if (!string.IsNullOrEmpty(path) && !dict.ContainsKey(path))
                    {
                        dict.Add(path,null);
                        tasks.Add(LoadOneAsync(path));
                    }
                }
                var list2 = MonsterConfigCategory.Instance.GetAllList();
                for (int i = 0; i < list2.Count; i++)
                {
                    string path = list2[i].PoseFSM;
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
                dict[path] = JsonHelper.FromJson<ConfigFsmController>(jStr);
                return;
            }
#if RoslynAnalyzer
            else
            {
                var bytes = await ResourcesManager.Instance.LoadConfigBytesAsync(path);
                Unity.Code.NinoGen.Deserializer.Deserialize(bytes,out ConfigFsmController res);
                dict[path] = res;
                return;
            }
#endif
            Log.Error($"GetConfigFsmController 失败，ConfigType = {Define.ConfigType} 未处理");
        }

        public void Destroy()
        {
            Instance = null;
            dict = null;
            _list = null;
        }

        #endregion

        public ConfigFsmController Get(string path)
        {
            this.dict.TryGetValue(path, out ConfigFsmController item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof(ConfigFsmController)}，配置id: {path}");
            }

            return item;
        }

        public Dictionary<string, ConfigFsmController> GetAll()
        {
            return dict;
        }

        public List<ConfigFsmController> GetAllList()
        {
            return _list;
        }
    }
}