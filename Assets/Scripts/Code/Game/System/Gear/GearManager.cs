using System.Collections.Generic;

namespace TaoTie
{
    public class GearManager: IManager<List<ConfigGear>,SceneManagerProvider>
    {
        #region IManager

        public void Init(List<ConfigGear> gears,SceneManagerProvider scene)
        {
            this.Parent = scene;
            configIdMapGear = new Dictionary<ulong, long>();
            if (gears != null)
            {
                for (int i = 0; i < gears.Count; i++)
                {
                    var gearConf = gears[i];
                    if (gearConf == null)
                    {
                        Log.Error("配置为空！请策划检查");
                        continue;
                    }


                    if (configIdMapGear.ContainsKey(gearConf.id))
                    {
                        Log.Error("GearConfigId重复 " + gearConf.id + "！请策划检查");
                        continue;
                    }

                    var gear = scene.GetManager<EntityManager>().CreateEntity<Gear, ConfigGear>(gearConf);
                    configIdMapGear.Add(gearConf.id, gear.Id);
                    Log.Info("<color=red>创建gear</color>"+gearConf.id);
                }
            } 
        }

        public void Destroy()
        {
            configIdMapGear.Clear();
            configIdMapGear = null;
            Parent = null;
        }

        #endregion
        
        private Dictionary<ulong, long> configIdMapGear;
        public SceneManagerProvider Parent { get; private set; }
        /// <summary>
        /// 通过配置Id获取Gear，注意不是Gear的Id！
        /// </summary>
        /// <param name="gearConfigId">配置Id</param>
        /// <param name="gear"></param>
        /// <returns></returns>
        public bool TryGetGear(ulong gearConfigId, out Gear gear)
        {
            if (this.configIdMapGear.TryGetValue(gearConfigId, out var gearid))
            {
                gear = this.Parent.GetManager<EntityManager>().Get<Gear>(gearid);
                return true;
            }
            gear = null;
            return false;
        }
    }
}