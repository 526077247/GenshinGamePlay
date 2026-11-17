using UnityEngine;

namespace TaoTie
{
    public class DoActionOnInputMixin : AbilityMixin<ConfigDoActionOnInputMixin>,IUpdate
    {
        private Entity owner;

        private long timerId;
        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnInputMixin config)
        {
            owner = actorAbility.Parent.GetParent<Entity>();
            timerId = TimerManager.Instance.NewFrameTimer(TimerType.ComponentUpdate, this);
        }

        public void Update()
        {
            // 边沿触发：按住不放不再每帧重复发起施法请求（一次按下 = 一次意图）。
            // GetKeyDown 内部即 keyStatus & KeyDown，移动端触摸也被折成 KeyDown，两平台行为一致。
            // passed 受 IgnoreUI 约束（普攻 IgnoreUI=true 查 IsPointerOverUI）；被 UI 遮挡时按键被吞掉。
            bool passed = InputManager.Instance.GetKeyDown(Config.KeyCode, Config.IgnoreUI);
            if (passed)
            {
                ExecuteAction(owner);
            }
            else if (SkillComponent.EnableCancelOnRelease && InputManager.Instance.GetKeyUp(Config.KeyCode))
            {
                CancelBufferedSkill(owner);
            }
        }

        /// <summary>
        /// 松手即撤销本键派发出的缓冲技能意图。
        /// 撤销侧刻意不传 IgnoreUI —— 指针松手时若恰好停在 UI 上，带 UI 门会漏撤销，
        /// 留下"人已经松手了却还在自动补发"。撤销只关心原始按键状态。
        /// 只处理 TryDoSkill：一个键派发的其它 action（音效/UI）不产生 pending。
        /// </summary>
        private void CancelBufferedSkill(Entity target)
        {
            var skillC = target.GetComponent<SkillComponent>();
            if (skillC == null) return;
            var actions = Config.Actions;
            if (actions == null) return;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is TryDoSkill doSkill)
                {
                    if (doSkill.IsSlot) skillC.CancelPendingBySlot(doSkill.Slot);
                    else skillC.CancelPendingByConfigId(doSkill.ConfigId);
                }
            }
        }
        
        private void ExecuteAction(Entity target)
        {
            var actions = Config.Actions;
            if (actions != null)
            {
                var executer = GetActionExecuter();
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].DoExecute(executer, actorAbility, actorModifier, target);
                }
            }
        }


        protected override void DisposeInternal()
        {
            TimerManager.Instance.Remove(ref timerId);
            owner = null;
        }
    }
}