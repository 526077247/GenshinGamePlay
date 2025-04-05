using UnityEngine;

namespace TaoTie
{
    public abstract class SceneEntity: Entity
    {
        private Vector3 position; //坐标
        /// <summary>
        /// 有挂点是localPosition，无挂点为worldPosition
        /// </summary>
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
            this.position = value;
        }
        public Vector3 Forward
        {
            get => this.Rotation * Vector3.forward;
            set => this.Rotation = Quaternion.LookRotation(value, Vector3.up);
        }
        public Vector3 Up => this.Rotation * Vector3.up;
        private Quaternion rotation;
        /// <summary>
        /// 有挂点是localRotation，无挂点为worldRotation
        /// </summary>
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
        public void SyncViewRotation(Quaternion value)
        {
            this.rotation = value;
        }
        private Vector3 localScale = Vector3.one;
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
        public void SyncViewLocalScale(Vector3 value)
        {
            this.localScale = value;
        }
    }
}