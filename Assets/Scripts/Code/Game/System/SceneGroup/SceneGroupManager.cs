using System.Collections.Generic;

namespace TaoTie
{
    public class SceneGroupManager: IManager<ulong[],SceneManagerProvider>
    {
        #region IManager

        public void Init(ulong[] ids,SceneManagerProvider scene)
        {
            this.Parent = scene;
            EntityManager em = scene.GetManager<EntityManager>();
            configIdMapSceneGroup = new Dictionary<ulong, long>();
            for (int i = 0; i < (ids?.Length ?? 0); i++)
            {
                var sceneGroupConf = ConfigSceneGroupCategory.Instance.Get(ids[i]);
                if (sceneGroupConf == null)
                {
                    Log.Error("配置为空！请策划检查");
                    continue;
                }


                if (configIdMapSceneGroup.ContainsKey(sceneGroupConf.Id))
                {
                    Log.Error("SceneGroupConfigId重复 " + sceneGroupConf.Id + "！请策划检查");
                    continue;
                }

                var sceneGroup = em.CreateEntity<SceneGroup, ConfigSceneGroup, SceneGroupManager>(sceneGroupConf, this);
                configIdMapSceneGroup.Add(sceneGroupConf.Id, sceneGroup.Id);
                Log.Info("<color=red>创建SceneGroup</color>" + sceneGroupConf.Id);
            }
        }

        public void Destroy()
        {
            configIdMapSceneGroup.Clear();
            configIdMapSceneGroup = null;
            Parent = null;
        }

        #endregion
        
        private Dictionary<ulong, long> configIdMapSceneGroup;
        public SceneManagerProvider Parent { get; private set; }
        /// <summary>
        /// 通过配置Id获取SceneGroup，注意不是SceneGroup的Id！
        /// </summary>
        /// <param name="sceneGroupConfigId">配置Id</param>
        /// <param name="sceneGroup"></param>
        /// <returns></returns>
        public bool TryGetSceneGroup(ulong sceneGroupConfigId, out SceneGroup sceneGroup)
        {
            if (this.configIdMapSceneGroup.TryGetValue(sceneGroupConfigId, out var sceneGroupId))
            {
                sceneGroup = this.Parent.GetManager<EntityManager>().Get<SceneGroup>(sceneGroupId);
                return true;
            }
            sceneGroup = null;
            return false;
        }
    }
}