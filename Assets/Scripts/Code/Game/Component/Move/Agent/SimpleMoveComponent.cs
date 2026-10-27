using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>
    {
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

        // 击退：此前本类完全不消费 CharacterInput.HitImpulse，被击退时毫无反应（配置了也不生效）
        protected override float HitImpulseToSpeed => ConfigMove.HitImpulseScale;
        protected override float HitImpulseDecayRate => ConfigMove.HitImpulseDecay;

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

            // 击退通道：本类没有重力与着地概念，竖直分量会让单位永久漂浮（上去不下来），
            // 因此只吃水平分量；抬升需求由动画移动的资产承担。
            ConsumeHitImpulse();
            HitLaunched = false; // 本类没有着地概念，不存在「击飞」状态
            DecayHitVelocity(deltaTime);
            hitVelocity.y = 0f;
            velocity += hitVelocity;

            CharacterInput.Velocity = velocity;
            if (velocity != Vector3.zero)
            {
                Vector3 displacement = deltaTime * velocity;
                if (!ConfigMove.CanPassWall)
                {
                    var area = GetComponent<MoveAreaComponent>()?.Area;
                    displacement = Constrain(displacement, area);
                }
                displacement = ConstrainActor(displacement);
                SceneEntity.Position += displacement;
            }

            ResolveActorContacts();
        }
    }
}
