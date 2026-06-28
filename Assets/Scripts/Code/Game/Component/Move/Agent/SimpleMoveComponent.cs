using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>
    {
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        protected override void InitInternal()
        {
            
        }

        protected override void DestroyInternal()
        {
            
        }
        protected override void UpdateInternal()
        {
            if(CharacterInput == null) return;
            float deltaTime = GameTimerManager.Instance.GetDeltaTime() / 1000f;
            HandleRotation(deltaTime);
            
            //doMove
            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed * CharacterInput.SpeedScale;
            velocity = ApplyORCA(velocity, speed);

            CharacterInput.Velocity = velocity;
            if (velocity != Vector3.zero)
            {
                SceneEntity.Position += deltaTime * velocity;
            }
        }
    }
}