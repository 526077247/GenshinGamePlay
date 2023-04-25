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
        }

        public void Destroy()
        {
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
    }
}