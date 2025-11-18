using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 场景单位
    /// </summary>
    public abstract class Unit : SceneEntity
    {
        #region 基础数据

        public int ConfigId { get; protected set; } //配置表id

        public UnitConfig Config => UnitConfigCategory.Instance.Get(this.ConfigId);

        #endregion
        protected ConfigFsmController GetFsmConfig(string path)
        {
            return ConfigFsmControllerCategory.Instance.Get(path);
        }
        protected ConfigAIBeta GetAIConfig(string path)
        {
            return ConfigAIBetaCategory.Instance.Get(path);
        }
        
        protected ConfigActor GetActorConfig(string path)
        {
            return ConfigActorCategory.Instance.Get(path);
        }
    }
}