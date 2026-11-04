
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 玩家的输入
    /// </summary>
    public class LocalInputController : Component, IComponent, IUpdate
    {
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();

        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        private bool canMove = true;
        private bool canTurn = true;
        private long lastCheckTime;
        private Vector3 lastDirection;
        private bool isInputFrozen = false;
        private Vector3 frozenDirection;

        /// <summary>
        /// 施法瞬间按着方向键时置位：这些键被"当作没按"，直到玩家松手。
        /// 只影响按下瞬间就在按住的那批键 —— 松手后再按是新的意图，会正常产生移动意图，
        /// 从而触发吟唱状态的"主动移动打断"。见 <see cref="SuppressMoveUntilRelease"/>。
        /// </summary>
        private bool suppressMoveUntilRelease;
        
        private ControlComponent controlComponent;
        
        #region IComponent

        public void Init()
        {
            canMove = fsm.DefaultFsm.CurrentState.CanMove;
            canTurn = fsm.DefaultFsm.CurrentState.CanTurn;
            suppressMoveUntilRelease = false;
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

        public void Update()
        {
            // 方向先读出来：冲刺（包括打断吟唱的那一次）要用它转向，
            // 而下面的 deny 分支在算方向之前就会 return。
            Vector3 direction = ReadMoveDirection();

            // 被控制时连"意图"都不产生：不仅停移动，也一并阻断跳跃/转向。
            // 但冲刺要单独过一次控制位判定 —— 吟唱这类"自身施法"只 Deny Move|CastSkill，
            // 冲刺正是打断它的手段；硬控（CrowdControl）才会把 Sprint 位一起剥夺。
            if (!CanMoveNow())
            {
                if (InputManager.Instance.GetKey(GameKeyCode.SprintBS) && CanSprintNow())
                {
                    TrySprint(direction);
                }
                ClearMoveIntent();
                return;
            }
            
            if (suppressMoveUntilRelease)
            {
                if (direction == Vector3.zero)
                {
                    suppressMoveUntilRelease = false;
                    lastDirection = Vector3.zero;
                    lastCheckTime = 0;
                }
                else
                {
                    ClearMoveIntent();
                    return;
                }
            }

            //断触问题
            if (direction == Vector3.zero)
            {
                if (lastCheckTime + 100 > TimerManager.Instance.GetTimeNow())
                {
                    direction = lastDirection;
                }
            }
            else
            {
                lastDirection = direction;
                lastCheckTime = TimerManager.Instance.GetTimeNow();
            }

            if (isInputFrozen)
            {
                direction = frozenDirection;
            }
            
            if (InputManager.Instance.GetKey(GameKeyCode.SprintBS))
            {
                TrySprint(direction);
            }
            else
            {
                TryJump();
            }
            this.TryMove(direction);
        }

        /// <summary>
        /// 读方向键 / 摇杆，合成世界方向。不含"断触补帧"和输入冻结 —— 那两步只属于正常移动路径。
        /// </summary>
        private Vector3 ReadMoveDirection()
        {
            Vector3 direction = Vector3.zero;
            if (InputManager.Instance.GetKey(GameKeyCode.MoveForward))
            {
                direction += Vector3.forward;
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveBack))
            {
                direction += Vector3.back;
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveLeft))
            {
                direction += Vector3.left;
            }
            if (InputManager.Instance.GetKey(GameKeyCode.MoveRight))
            {
                direction += Vector3.right;
            }

            if (direction == Vector3.zero)
            {
                if (InputAxisBind.AxisBind.TryGetValue(AxisType.Horizontal, out float x)
                    && InputAxisBind.AxisBind.TryGetValue(AxisType.Vertical, out float y))
                {
                    direction += Vector3.forward * y;
                    direction += Vector3.right * x;
                }
            }

            return direction;
        }

        /// <summary>
        /// 冲刺。只置 FSM trigger，位移由状态的动画驱动。
        ///
        /// 两个入口共用一份实现：正常移动（<see cref="Update"/> 尾部）与吟唱期间被打断时
        /// （被 Deny Move 挡住、但 Sprint 位没被剥夺）。CD、朝向、冷却触发都只在这里，
        /// 不要在别处再拼一套 —— 否则手感参数会分叉。
        /// </summary>
        private void TrySprint(Vector3 direction)
        {
            var skillC = GetComponent<SkillComponent>();
            if (skillC?.IsSprintBSInCD() == true) return;

            if (direction != Vector3.zero && moveComponent != null)
            {
                moveComponent.ForceLookTo(direction);
            }
            fsm.SetData(FSMConst.SprintBS, true);
            skillC?.TriggerSprintBSCD();

            // 冲刺是明确的"新意图"，把施法瞬间的输入抑制清掉：
            // 否则冲完回到 idle，玩家还按着的方向键仍被当作没按，得先松手才能接着跑。
            suppressMoveUntilRelease = false;
            lastDirection = direction;
            lastCheckTime = direction == Vector3.zero ? 0 : TimerManager.Instance.GetTimeNow();
        }

        private void ClearMoveIntent()
        {
            fsm.SetData(FSMConst.MotionFlag, 0);
            fsm.SetData(FSMConst.MotionDirection, 0);
        }

        #endregion
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

        /// <summary>
        /// 现在是否允许冲刺。与 <see cref="CanMoveNow"/> 分开判定：
        /// 硬控按 <see cref="ControlFlag.CrowdControl"/> 连 Sprint 位一起剥夺（人钉死不能冲），
        /// 而吟唱这类自身施法只剥夺 Move|CastSkill —— 冲刺仍然可用，它就是打断吟唱的手段。
        /// </summary>
        private bool CanSprintNow()
        {
            if (controlComponent == null || controlComponent.IsDispose)
                controlComponent = parent.GetComponent<ControlComponent>();
            return controlComponent == null || controlComponent.CanDo(ControlFlag.Sprint);
        }
        public void TryMove(Vector3 direction, MotionFlag mFlag = MotionFlag.Run, MotionDirection mDirection = MotionDirection.Forward)
        {
            if (moveComponent == null) return;
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
                direction = direction.magnitude * moveComponent.CharacterInput.Direction;
            }
            if (direction == Vector3.zero)
            {
                if (fsm.GetInt(FSMConst.MotionFlag) != 0)
                {
                    Messager.Instance.Broadcast(Id, MessageId.CheckLeftOrRight);
                }
                fsm.SetData(FSMConst.MotionFlag, 0);
                fsm.SetData(FSMConst.MotionDirection, 0);
            }
            else
            {
                if (fsm.GetInt(FSMConst.MotionFlag) == 0)
                {
                    //随机一只脚
                    int isLeft = Random.Range(0, 2);
                    fsm.SetData(FSMConst.LeftFoot, isLeft);
                    
                    Messager.Instance.Broadcast(Id, MessageId.CheckAngleVF, direction);
                }
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
                if (CameraManager.Instance.MainCamera() != null && 
                    CameraManager.Instance.CurrentCameraState.Data.AvatarFaceDirection)
                {
                    moveComponent.CharacterInput.FaceDirection = CameraManager.Instance.MainCamera().transform.forward;
                }
                else
                {
                    moveComponent.CharacterInput.FaceDirection = Vector3.forward;
                }
                //因为是动画驱动移动，所以这里速度指的是动画速度缩放比例
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

        public void FreezeInput(Vector3 direction = default)
        {
            if(IsDispose) return;
            isInputFrozen = true;
            if (direction == default)
            {
                frozenDirection = lastDirection;
            }
            else
            {
                frozenDirection = direction;
            }
        }

        public void UnfreezeInput()
        {
            if(IsDispose) return;
            isInputFrozen = false;
        }

        /// <summary>
        /// 进入施法：把此刻还按住的方向键忽略掉，直到玩家松手。
        ///
        /// 为什么需要：FSM 里的吟唱状态是用 <c>MotionFlag != 0</c> 判"主动移动打断"的，
        /// 而移动意图来自输入。若玩家边跑边放技能，吟唱会在第 0 帧就被自己按着的方向键打断。
        /// 这里只吞"施法瞬间已按住"的键：松手后再按是明确的移动意图，照旧打断吟唱。
        /// </summary>
        public void SuppressMoveUntilRelease()
        {
            if (IsDispose) return;
            suppressMoveUntilRelease = true;
        }

        public void TryJump()
        {
            if (InputManager.Instance.GetKey(GameKeyCode.Jump) && fsm.DefaultFsm.CurrentState.CanJump)
            {
                Messager.Instance.Broadcast(Id, MessageId.CheckLeftOrRight);
                fsm.SetData(FSMConst.Jump, true);
            }
        }
    }
}