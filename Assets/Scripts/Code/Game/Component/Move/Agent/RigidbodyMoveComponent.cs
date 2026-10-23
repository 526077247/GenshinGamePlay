using UnityEngine;

namespace TaoTie
{
    public class RigidbodyMoveComponent : MoveComponent<ConfigRigidbodyMove>, IFixedUpdate
    {
        private Rigidbody rigidbody;
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

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