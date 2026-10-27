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
            if (ConfigActor.Move != null)
            {
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

            // 角色体积代理（互相阻挡 / 动量推挤）与移动方式无关：没有移动组件的单位（木桩）也要挡人。
            // 必须放在移动组件之后挂载 —— ActorBodyComponent.Init 用 MoveComponent 是否存在判定 CanMove。
            AddComponent<ActorBodyComponent>();
        }
    }
}