namespace TaoTie
{
    public class SceneGroupActorComponent: Component,IComponent<int,long>
    {
        public int LocalId { get; private set; }

        private long sceneGroupId;
        public SceneGroup SceneGroup => parent.Parent.Get<SceneGroup>(sceneGroupId);
        #region IComponent

        public void Init(int p1, long p2)
        {
            LocalId = p1;
            sceneGroupId = p2;
            Messager.Instance.AddListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
            SceneGroup?.OnActorRelease(LocalId);
            sceneGroupId = 0;
            LocalId = 0;
        }

        #endregion
        
        /// <summary>
        /// 与SceneGroup脱钩，之后SceneGroup切换suite，也不会销毁该actor
        /// </summary>
        public void RemoveFromSceneGroup()
        {
            SceneGroup?.OnActorRelease(LocalId);
        }

        private void OnBeKill(ConfigDie configDie, DieStateFlag flag)
        {
            Messager.Instance.Broadcast(sceneGroupId, MessageId.SceneGroupEvent, new AnyMonsterDieEvent()
            {
                ActorId = LocalId
            });
            Dispose();
        }
    }
}