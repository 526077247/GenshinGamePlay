using UnityEngine;

namespace TaoTie
{
    public class Effect : Entity, IEntity<string>,IEntity<string,long>
    {
        public override EntityType Type => EntityType.Effect;

        public string EffectName;
        
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
        public void Init(string name)
        {
            EffectName = name;
            AddComponent<GameObjectHolderComponent, string>($"Effect/{name}/Prefabs/{name}.prefab");
        }
        public void Init(string name, long delay)
        {
            EffectName = name;
            string path = $"Effect/{name}/Prefabs/{name}.prefab";
            if (delay <= 0)
            {
                AddComponent<GameObjectHolderComponent, string>(path);
            }
            else
            {
                GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(path,1).Coroutine();
                InitViewAsync(path, delay).Coroutine();
            }
        }

        private async ETTask InitViewAsync(string path, long delay)
        {
            await GameTimerManager.Instance.WaitAsync(delay);
            if(IsDispose) return;
            AddComponent<GameObjectHolderComponent, string>(path);
        }
        
        public void Destroy()
        {
            EffectName = null;
        }
    }
}