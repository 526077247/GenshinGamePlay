using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>
    {
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

        /// <summary>
        /// 墙体扫掠检测使用的碰撞层。可按配置重写。
        /// </summary>
        protected int WallCastingLayer => ConfigMove.WallLayer;

        protected override void InitInternal()
        {
            InitMoveConstraint(ActorMoveProfile.Create(parent as Actor), WallCastingLayer);
        }

        protected override void DestroyInternal()
        {
        }

        protected override void UpdateInternal()
        {
            if (!ConfigMove.CanPassWall)
                ResolveEmbedding();

            if (CharacterInput == null) return;
            float deltaTime = GameTimerManager.Instance.GetDeltaTime() / 1000f;
            HandleRotation(deltaTime);

            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed * CharacterInput.SpeedScale;
            velocity = ApplyORCA(velocity, speed);

            CharacterInput.Velocity = velocity;
            if (velocity != Vector3.zero)
            {
                Vector3 displacement = deltaTime * velocity;
                if (!ConfigMove.CanPassWall)
                {
                    var area = GetComponent<MoveAreaComponent>()?.Area;
                    displacement = Constrain(displacement, area);
                }
                SceneEntity.Position += displacement;
            }
        }
    }
}
