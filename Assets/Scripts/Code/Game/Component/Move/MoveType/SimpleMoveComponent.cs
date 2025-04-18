using Unity.Mathematics;
using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>, IUpdate
    {
        private ORCAAgentComponent orcaAgent => parent.GetComponent<ORCAAgentComponent>();
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        private FsmComponent FsmComponent => parent.GetComponent<FsmComponent>();
        protected override void InitInternal()
        {
            
        }

        protected override void DestroyInternal()
        {
            
        }

        public void Update()
        {
            if(CharacterInput == null) return;
            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed;
            orcaAgent?.SetVelocity(velocity, speed);
            if (orcaAgent != null)
            {
                velocity = orcaAgent.GetVelocity();
            }
            CharacterInput.Velocity = velocity;
            if (velocity != Vector3.zero)
            {
                SceneEntity.Position += (GameTimerManager.Instance.GetDeltaTime() / 1000f) * velocity;
                MoveStart();
            }
            else
            {
                MoveStop();
            }
        }
        
        private void MoveStart()
        {
            FsmComponent.SetData(FSMConst.MotionFlag, 1);
        }

        private void MoveStop()
        {
            FsmComponent.SetData(FSMConst.MotionFlag, 0);
        }
    }
}