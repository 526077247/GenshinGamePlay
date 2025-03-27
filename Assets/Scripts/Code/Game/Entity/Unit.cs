using System.Collections.Generic;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
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
        
        private Vector3 localScale = Vector3.zero;
        public Vector3 LocalScale
        {
            get => this.localScale;
            set
            {
                var oldScale = this.localScale;
                this.localScale = value;
                Messager.Instance.Broadcast(Id, MessageId.ChangeScaleEvt, this, oldScale);
            }
        }

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