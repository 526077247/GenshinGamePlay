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
            if (ConfigActor.Move.Agent is ConfigAnimatorMove am)
            {
                moveComponent =
                    AddComponent<AnimatorMoveComponent, ConfigAnimatorMove>(am, TypeInfo<MoveComponent>.Type);
            }
            else if (ConfigActor.Move.Agent is ConfigSimpleMove sm)
            {
                moveComponent = AddComponent<SimpleMoveComponent, ConfigSimpleMove>(sm, TypeInfo<MoveComponent>.Type);
            }
            else if (ConfigActor.Move.Agent is ConfigRigidbodyMove rm)
            {
                moveComponent = AddComponent<RigidbodyMoveComponent, ConfigRigidbodyMove>(rm, TypeInfo<MoveComponent>.Type);
            }
            
            moveComponent?.ChangeStrategy(ConfigActor.Move.DefaultStrategy);
        }
    }
}