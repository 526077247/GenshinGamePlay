using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupManager: IManager<ulong[],SceneManagerProvider>
    {
        #region IManager

        public void Init(ulong[] ids,SceneManagerProvider scene)
        {
            this.Parent = scene;
            configIdMapSceneGroup = new UnOrderMultiMap<ulong, long>();
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

                var sceneGroup = CreateSceneGroup(sceneGroupConf, null, null);
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
        
        private UnOrderMultiMap<ulong, long> configIdMapSceneGroup;
        public SceneManagerProvider Parent { get; private set; }

        /// <summary>
        /// 创建SceneGroup实体，可指定实例位置/朝向覆盖配置模板。仅创建，不注册映射。
        /// </summary>
        private SceneGroup CreateSceneGroup(ConfigSceneGroup sceneGroupConf, Vector3? position, Vector3? rotation)
        {
            if (position.HasValue || rotation.HasValue)
            {
                // 深拷贝模板，避免污染ConfigSceneGroupCategory中的配置
                sceneGroupConf = ProtobufHelper.FromBytes<ConfigSceneGroup>(ProtobufHelper.ToBytes(sceneGroupConf));
                if (position.HasValue) sceneGroupConf.Position = position.Value;
                if (rotation.HasValue) sceneGroupConf.Rotation = rotation.Value;
            }

            var em = Parent.GetManager<EntityManager>();
            return em.CreateEntity<SceneGroup, ConfigSceneGroup, SceneGroupManager>(sceneGroupConf, this);
        }

        /// <summary>
        /// 运行时根据配置模板动态添加SceneGroup（同一配置模板可添加多个实例）
        /// </summary>
        /// <param name="configId">ConfigSceneGroup配置Id</param>
        /// <param name="position">实例位置，为空则用模板配置</param>
        /// <param name="rotation">实例朝向(欧拉角)，为空则用模板配置</param>
        /// <returns></returns>
        public SceneGroup AddSceneGroup(ulong configId, Vector3? position = null, Vector3? rotation = null)
        {
            if (!ConfigSceneGroupCategory.Instance.GetAll().TryGetValue(configId, out var sceneGroupConf))
            {
                Log.Error("SceneGroup配置不存在 " + configId + "！请策划检查");
                return null;
            }

            if (sceneGroupConf.Disable)
            {
                Log.Error("SceneGroup配置已关闭 " + configId + "！请策划检查");
                return null;
            }

            var sceneGroup = CreateSceneGroup(sceneGroupConf, position, rotation);
            configIdMapSceneGroup.Add(configId, sceneGroup.Id);
            Log.Info("<color=red>动态创建SceneGroup</color>" + configId);
            return sceneGroup;
        }

        /// <summary>
        /// 运行时移除指定配置Id对应的所有SceneGroup实例（销毁其所有actor/zone）
        /// </summary>
        /// <param name="configId">ConfigSceneGroup配置Id</param>
        /// <returns></returns>
        public bool RemoveSceneGroup(ulong configId)
        {
            if (!configIdMapSceneGroup.TryGetValue(configId, out var list) || list == null || list.Count <= 0)
            {
                Log.Error("移除失败，不存在该SceneGroup " + configId);
                return false;
            }

            var ids = list.ToArray();
            configIdMapSceneGroup.Remove(configId);
            var em = Parent.GetManager<EntityManager>();
            for (int i = 0; i < ids.Length; i++)
            {
                em.Remove(ids[i]);
            }

            Log.Info("<color=red>动态移除SceneGroup</color>" + configId);
            return true;
        }

        /// <summary>
        /// 运行时移除指定的SceneGroup实例
        /// </summary>
        /// <param name="sceneGroup"></param>
        /// <returns></returns>
        public bool RemoveSceneGroup(SceneGroup sceneGroup)
        {
            if (sceneGroup == null || sceneGroup.IsDispose || sceneGroup.Config == null) return false;
            var configId = sceneGroup.Config.Id;
            if (!configIdMapSceneGroup.Remove(configId, sceneGroup.Id))
            {
                return false;
            }

            Parent.GetManager<EntityManager>().Remove(sceneGroup.Id);
            Log.Info("<color=red>动态移除SceneGroup实例</color>" + configId);
            return true;
        }

        /// <summary>
        /// 获取指定配置Id的全部活体SceneGroup实例
        /// </summary>
        /// <param name="configId">ConfigSceneGroup配置Id</param>
        /// <returns></returns>
        public SceneGroup[] GetSceneGroups(ulong configId)
        {
            if (!configIdMapSceneGroup.TryGetValue(configId, out var list) || list == null || list.Count <= 0)
            {
                return Array.Empty<SceneGroup>();
            }

            var em = Parent.GetManager<EntityManager>();
            var res = new SceneGroup[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                res[i] = em.Get<SceneGroup>(list[i]);
            }

            return res;
        }
        
        /// <summary>
        /// 通过配置Id获取SceneGroup（同配置模板多实例时取第一个实例），注意不是SceneGroup的Id！
        /// </summary>
        /// <param name="sceneGroupConfigId">配置Id</param>
        /// <param name="sceneGroup"></param>
        /// <returns></returns>
        public bool TryGetSceneGroup(ulong sceneGroupConfigId, out SceneGroup sceneGroup)
        {
            var sceneGroupId = this.configIdMapSceneGroup.GetOne(sceneGroupConfigId);
            if (sceneGroupId != 0)
            {
                sceneGroup = this.Parent.GetManager<EntityManager>().Get<SceneGroup>(sceneGroupId);
                if (sceneGroup != null)
                {
                    return true;
                }
            }
            sceneGroup = null;
            return false;
        }
    }
}