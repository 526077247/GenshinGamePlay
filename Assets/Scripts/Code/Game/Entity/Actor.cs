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
            if(ConfigActor.Move == null) return;
            MoveComponent moveComponent = null;
            if (ConfigActor.Move.Strategy is ConfigAnimatorMove am)
            {
                moveComponent =
                    AddComponent<AnimatorMoveComponent, ConfigAnimatorMove>(am, TypeInfo<MoveComponent>.Type);
            }
            else if (ConfigActor.Move.Strategy is ConfigSimpleMove sm)
            {
                moveComponent = AddComponent<SimpleMoveComponent, ConfigSimpleMove>(sm, TypeInfo<MoveComponent>.Type);
            }

            if (moveComponent != null)
            {
                if (ConfigActor.Move.DefaultAgent is ConfigPlatformMove pm)
                {
                    if (pm.Route != null)
                    {
                        moveComponent.SetRoute(pm.Route,pm.Delay);
                    }
                }
                else if (ConfigActor.Move.DefaultAgent is ConfigFollowMove fm)
                {
                    moveComponent.SetConfigFollowMove(fm);
                }
            }
        }
    }
}