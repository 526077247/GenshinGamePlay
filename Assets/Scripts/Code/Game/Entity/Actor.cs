namespace TaoTie
{
    public abstract class Actor: Unit
    {
        /// <summary>
        /// 阵营id
        /// </summary>
        public uint CampId;
        
        public ConfigActor ConfigActor { get; protected set; }

        protected void CreateMoveComponent()
        {
            if (ConfigActor.Move is ConfigAnimatorMove am)
            {
                AddComponent<AnimatorMoveComponent, ConfigAnimatorMove>(am);
            }
            else if (ConfigActor.Move is ConfigPlatformMove pm)
            {
                var pc = AddComponent<PlatformMoveComponent, ConfigRoute, SceneGroup>(pm.Route, null);
                if (pm.Route != null && pm.Delay >= 0)
                {
                    pc.DelayStart(pm.Delay);
                }
            }
            else if (ConfigActor.Move is ConfigFollowMove fm)
            {
                AddComponent<FollowMoveComponent, ConfigFollowMove>(fm);
            }
        }
    }
}