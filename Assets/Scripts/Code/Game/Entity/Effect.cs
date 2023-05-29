using UnityEngine;

namespace TaoTie
{
    public class Effect : Entity, IEntity<string>
    {
        public override EntityType Type => EntityType.Effect;
        
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

        public void Init(string name)
        {
            AddComponent<GameObjectHolderComponent, string>($"Effect/{name}/Prefabs/{name}.prefab");
        }

        public void Destroy()
        {
            
        }
    }
}