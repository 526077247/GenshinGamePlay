using UnityEngine;

namespace TaoTie
{
    public class RigidbodyMoveComponent : MoveComponent<ConfigRigidbodyMove>, IFixedUpdate
    {
        private Rigidbody rigidbody;
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

        /// <summary>
        /// 刚体移动<b>暂不消费击退</b>：速度由 Rigidbody 直接接管，而且 isKinematic 会随速度归零来回切，
        /// 再叠一份 hitVelocity 进去会与该切换互相打架（表现为被击退后僵在半空或原地抽搐）。
        /// 这里显式写 0，让「配了击退没反应」落到 <see cref="MoveComponent.ApplyHitImpulse"/> 的告警里，
        /// 而不是让基类默认值悄悄吞掉。
        /// </summary>
        protected override float HitImpulseToSpeed => 0f;

        protected override void InitInternal()
        {
            InitMoveConstraint(ActorMoveProfile.Create(parent as Actor), 0);
            InitInternalAsync().Coroutine();
        }

        protected override void DestroyInternal()
        {
            rigidbody = null;
        }

        private async ETTask InitInternalAsync()
        {
            var um = parent.GetComponent<UnitModelComponent>();
            await um.WaitLoadGameObjectOver();
            rigidbody = um.EntityView.GetComponent<Rigidbody>();
        }

        protected override void UpdateInternal()
        {
        }

        public void FixedUpdate()
        {
            if (CharacterInput == null || rigidbody == null) return;
            float deltaTime = Time.fixedDeltaTime;
            if (deltaTime <= 0f) return;

            SceneEntity.SyncViewPosition(rigidbody.position);
            HandleRotation(deltaTime);

            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed * CharacterInput.SpeedScale;
            velocity = ApplyORCA(velocity, speed);
            velocity = ConstrainVelocity(velocity);

            CharacterInput.Velocity = velocity;
            rigidbody.velocity = velocity;
            rigidbody.isKinematic = velocity == Vector3.zero;
            SceneEntity.SyncViewPosition(rigidbody.position);
        }

        private Vector3 ConstrainVelocity(Vector3 velocity)
        {
            if (!HasMoveConstraint || velocity == Vector3.zero) return velocity;
            float deltaTime = Time.fixedDeltaTime;
            if (deltaTime <= 0f) return velocity;

            var area = GetComponent<MoveAreaComponent>()?.Area;
            Vector3 displacement = ConstrainTerrain(velocity * deltaTime, area);
            return displacement / deltaTime;
        }
    }
}