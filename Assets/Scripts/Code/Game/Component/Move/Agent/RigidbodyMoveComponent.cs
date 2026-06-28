using UnityEngine;

namespace TaoTie
{
    public class RigidbodyMoveComponent : MoveComponent<ConfigRigidbodyMove>
    {
        private Rigidbody rigidbody;
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

        protected override void InitInternal()
        {
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
            if (CharacterInput == null || rigidbody == null) return;
            float deltaTime = GameTimerManager.Instance.GetDeltaTime() / 1000f;
            HandleRotation(deltaTime);

            //doMove
            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed * CharacterInput.SpeedScale;
            velocity = ApplyORCA(velocity, speed);

            CharacterInput.Velocity = velocity;
            rigidbody.velocity = velocity;
            rigidbody.isKinematic = velocity == Vector3.zero;
            SceneEntity.SyncViewPosition(rigidbody.position);
        }
    }
}