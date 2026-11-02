using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// AI输入
    /// </summary>
    public class AIInputController: Component, IComponent
    {
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

        private bool canMove = true;
        private bool canTurn = true;
        private ControlComponent controlComponent;

        #region IComponent

        public void Init()
        {
            canMove = fsm.DefaultFsm.CurrentState.CanMove;
            canTurn = fsm.DefaultFsm.CurrentState.CanTurn;
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
            // 组件池化复用：不清会把上一个实体的 ControlComponent 带到下一个实体上
            controlComponent = null;
        }

        
        #endregion
        
        public void TryMove(Vector3 direction, MotionFlag mFlag = MotionFlag.Run, MotionDirection mDirection = MotionDirection.Forward)
        {
            if (!CanMoveNow())
            {
                fsm.SetData(FSMConst.MotionFlag, 0);
                fsm.SetData(FSMConst.MotionDirection, 0);
                return;
            }
            if (!canTurn)
            {
                if (mDirection == MotionDirection.Left || mDirection == MotionDirection.Right)
                {
                    mDirection = MotionDirection.Forward;
                }
                direction = new Vector3(0, 0, direction.z);
            }
            if (direction == Vector3.zero)
            {
                fsm.SetData(FSMConst.MotionFlag, 0);
                fsm.SetData(FSMConst.MotionDirection, 0);
            }
            else
            {
                fsm.SetData(FSMConst.MotionFlag, (int)mFlag);
                fsm.SetData(FSMConst.MotionDirection, (int)mDirection);
            }

            if (moveComponent?.CharacterInput != null)
            {
                moveComponent.CharacterInput.MotionDirection = mDirection;
                if (canMove)
                    moveComponent.CharacterInput.Direction = direction.normalized;
                else
                    moveComponent.CharacterInput.Direction = Vector3.zero;
                moveComponent.CharacterInput.FaceDirection = direction.normalized;
                //因为是动画驱动移动，所以这里速度指的是速度比例
                moveComponent.CharacterInput.SpeedScale = numericComponent.GetAsFloat(NumericType.Speed);
            }
        }
        
        private void SetCanMove(bool canMove)
        {
            if(IsDispose) return;
            this.canMove = canMove;
        }

        private void SetCanTurn(bool canTurn)
        {
            if(IsDispose) return;
            this.canTurn = canTurn;
        }
        
        /// <summary>
        /// 现在是否允许主动位移。判定统一走 <see cref="ControlComponent"/> 的能力位掩码，
        /// 不再按"被哪种控制命中"来判断 —— 定身/击飞/恐惧等新控制接进来时这里不用改。
        /// </summary>
        private bool CanMoveNow()
        {
            if (controlComponent == null || controlComponent.IsDispose)
                controlComponent = parent.GetComponent<ControlComponent>();
            return controlComponent == null || controlComponent.CanDo(ControlFlag.Move);
        }
    }
}