using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class DelayDoAction: ConfigAbilityAction
    {
        [ProtoMember(10)][LabelText("延时调用(ms)")][NotNull]
        public BaseValue Delay;

        [ProtoMember(11)]
        public ConfigAbilityAction[] Actions;

        /// <summary>
        /// 到期时要求身上仍有这个 modifier，否则放弃执行。
        /// </summary>
        [ProtoMember(12)][LabelText("到期时要求存在的modifier")]
        public string RequireModifier;

        /// <summary>
        /// 到期时比对 ability 的这个 special 是否还是启动时的值，变了就放弃。
        /// </summary>
        [ProtoMember(13)][LabelText("到期时比对的序号special")]
        public string SeqSpecialKey;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if(Actions==null) return;
            int delay = ResolveDelay(actionExecuter, ability);
            // 解析出 0 通常意味着 special 没注入或公式写错。
            // 这里夹到 1ms 而不是直接 return：直接跳过会让吟唱永远等不到 CastDone，
            // 整场战斗卡在吟唱状态，比"变成瞬发"更难查。夹到 1 至少流程能走完，同时留一条警告。
            if (delay <= 0)
            {
                Log.Warning("DelayDoAction 延时解析为 " + delay + "ms，已按 1ms 处理，请检查 Delay 配置");
                delay = 1;
            }
            ExecuteAsync(actionExecuter, ability, modifier, target, delay, ResolveSeq(ability)).Coroutine();
        }

        /// <summary>
        /// BaseValue.Resolve 需要施法者来算属性/公式，所以传 actionExecuter；
        /// AbilityValue 会忽略这个参数，只读 ability 的 special
        /// </summary>
        private int ResolveDelay(Entity actionExecuter, ActorAbility ability)
        {
            if (Delay == null) return 1;
            return (int) Delay.Resolve(actionExecuter, ability);
        }

        private float ResolveSeq(ActorAbility ability)
        {
            if (string.IsNullOrEmpty(SeqSpecialKey) || ability == null) return 0;
            return ability.GetReplaceValue(SeqSpecialKey);
        }

        /// <summary>
        /// 常驻 ability 下判断"这笔延迟还算不算数"的唯一手段
        /// </summary>
        private static bool HasModifier(Entity entity, string modifierName)
        {
            var ac = entity.GetComponent<AbilityComponent>();
            if (ac?.Modifiers == null) return false;
            for (var node = ac.Modifiers.Last; node != null; node = node.Previous)
            {
                if (node.Value?.Config != null && node.Value.Config.ModifierName == modifierName)
                {
                    return true;
                }
            }
            return false;
        }

        private async ETTask ExecuteAsync(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target, int delay, float seq)
        {
            await GameTimerManager.Instance.WaitAsync(delay);
            if (ability.IsDispose || (modifier != null && modifier.IsDispose) ||
                actionExecuter.IsDispose || target.IsDispose) return;

            // 这笔延迟属于上一轮
            if (!string.IsNullOrEmpty(SeqSpecialKey) && ability.GetReplaceValue(SeqSpecialKey) != seq) return;

            // 被打断
            if (!string.IsNullOrEmpty(RequireModifier) && !HasModifier(actionExecuter, RequireModifier)) return;

            for (int i = 0; i < Actions.Length; i++)
            {
                Actions[i].DoExecute(actionExecuter, ability, modifier, target);
            }
        }
    }
}