using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>
    {
        protected override bool useAnimMove => false;
        private ORCAAgentComponent orcaAgent => parent.GetComponent<ORCAAgentComponent>();
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
            //doRotate
            if (CharacterInput.RotateSpeed > 0 && CharacterInput.Direction!= Vector3.zero)
            {
                var euler = SceneEntity.Rotation.eulerAngles;
                var dir = Quaternion.LookRotation(CharacterInput.Direction, SceneEntity.Up);
                var euler2 = dir.eulerAngles;
                var angle = euler2.y - euler.y;
                while (angle < -180)
                {
                    angle += 360;
                }

                while (angle > 180)
                {
                    angle -= 360;
                }

                if (Mathf.Abs(angle) > CharacterInput.RotateSpeed * 0.01f)
                {
                    var deltaAngle = CharacterInput.RotateSpeed * deltaTime * (angle < 0 ? -1 : 1);
                    float newY = euler.y + (angle < 0 ? Mathf.Max(deltaAngle, angle) : Mathf.Min(deltaAngle, angle));
                    SceneEntity.Rotation = Quaternion.Euler(euler.x, newY, euler.z);
                }
                else
                {
                    SceneEntity.Rotation = Quaternion.Euler(euler.x, euler2.y, euler.z);
                }
            }
            
            //doMove
            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed * CharacterInput.SpeedScale;
            orcaAgent?.SetVelocity(velocity, speed);
            if (orcaAgent != null)
            {
                velocity = orcaAgent.GetVelocity();
            }
            CharacterInput.Velocity = velocity;
            if (velocity != Vector3.zero)
            {
                SceneEntity.Position += deltaTime * velocity;
            }
        }
    }
}