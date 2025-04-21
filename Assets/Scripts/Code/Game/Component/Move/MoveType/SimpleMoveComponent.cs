using Unity.Mathematics;
using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>, IUpdate
    {
        private ORCAAgentComponent orcaAgent => parent.GetComponent<ORCAAgentComponent>();
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        protected override void InitInternal()
        {
            
        }

        protected override void DestroyInternal()
        {
            
        }

        public void Update()
        {
            if(CharacterInput == null) return;
            float deltaTime = (GameTimerManager.Instance.GetDeltaTime() / 1000f);
            //doRotate
            if (CharacterInput.RotateSpeed > 0)
            {
                var euler = SceneEntity.Rotation.eulerAngles;
                switch (CharacterInput.RotAngleType)
                {
                    case RotAngleType.ROT_ANGLE_X:
                        SceneEntity.Rotation = quaternion.Euler(euler.x + CharacterInput.RotateSpeed * deltaTime,
                            euler.y, euler.z);
                        break;
                    case RotAngleType.ROT_ANGLE_Y:
                        SceneEntity.Rotation = quaternion.Euler(euler.x,
                            euler.y + CharacterInput.RotateSpeed * deltaTime, euler.z);
                        break;
                    case RotAngleType.ROT_ANGLE_Z:
                        SceneEntity.Rotation = quaternion.Euler(euler.x, euler.y,
                            euler.z + CharacterInput.RotateSpeed * deltaTime);
                        break;
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