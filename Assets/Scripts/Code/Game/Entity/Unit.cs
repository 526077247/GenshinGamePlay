using System.Collections.Generic;
using Unity.Code.NinoGen;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 场景单位
    /// </summary>
    public abstract class Unit : Entity
    {
        #region 基础数据

        public int ConfigId { get; protected set; } //配置表id

        public UnitConfig Config => UnitConfigCategory.Instance.Get(this.ConfigId);

        private Vector3 position; //坐标

        
        public Vector3 Position
        {
            get => this.position;
            set
            {
                var oldPos = this.position;
                this.position = value;
                Messager.Instance.Broadcast(Id, MessageId.ChangePositionEvt, this, oldPos);
            }
        }

        public void SyncViewPosition(Vector3 value)
        {
            var oldPos = this.position;
            this.position = value;
            // Messager.Instance.Broadcast(Id, MessageId.ChangePositionEvt, this, oldPos);
        }
        public Vector3 Forward
        {
            get => this.Rotation * Vector3.forward;
            set => this.Rotation = Quaternion.LookRotation(value, Vector3.up);
        }
        public Vector3 Up => this.Rotation * Vector3.up;
        private Quaternion rotation;
        public Quaternion Rotation
        {
            get => this.rotation;
            set
            {
                var oldRot = this.rotation;
                this.rotation = value;
                Messager.Instance.Broadcast(Id, MessageId.ChangeRotationEvt, this, oldRot);
            }
        }

        #endregion
        protected ConfigFsmController GetFsmConfig(string path)
        {
            if (Define.ConfigType == 0)
            {
                var jStr = ResourcesManager.Instance.LoadConfigJson(path);
                return JsonHelper.FromJson<ConfigFsmController>(jStr);
            }
            else
            {
                var bytes = ResourcesManager.Instance.LoadConfigBytes(path);
                Deserializer.Deserialize(bytes,out ConfigFsmController res);
                return res;
            }
        }
        protected ConfigAIBeta GetAIConfig(string path)
        {
            if (Define.ConfigType == 0)
            {
                var jStr = ResourcesManager.Instance.LoadConfigJson(path);
                return JsonHelper.FromJson<ConfigAIBeta>(jStr);
            }
            else
            {
                var bytes = ResourcesManager.Instance.LoadConfigBytes(path);
                Deserializer.Deserialize(bytes,out ConfigAIBeta res);
                return res;
            }
        }
        
        protected ConfigActor GetActorConfig(string path)
        {
            if (Define.ConfigType == 0)
            {
                var jStr = ResourcesManager.Instance.LoadConfigJson(path);
                return JsonHelper.FromJson<ConfigActor>(jStr);
            }
            else
            {
                var bytes = ResourcesManager.Instance.LoadConfigBytes(path);
                Deserializer.Deserialize(bytes,out ConfigActor res);
                return res;
            }
        }
    }
}