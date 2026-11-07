#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    public enum StackingType
    {
        [LabelText("同时存在唯一一个")]
        Unique,
        [LabelText("互相独立存在")]
        Multiple,
        [LabelText("刷新已存在的modifier")]
        Refresh,
        [LabelText("延长已存在的modifier")]
        Prolong,        
        /// <summary>
        /// 单实例叠层：每次再施加同一 modifier 时层数 +1（上限查标记表），
        /// Properties 按层数放大，时长取最后一次刷新。
        ///
        /// 与 <see cref="Multiple"/> 的区别是"层的时长语义"：
        /// Multiple 是 N 个实例各自倒计时（每层独立到期，自然衰减）；
        /// Stack 只有一份计时器，要衰减得自己写（如每 3 秒掉一层）。
        /// 灼烧用 Stack（要"5 层引爆"和逐层衰减），中毒用 Multiple（各自跳各自的伤害）。
        /// </summary>
        [LabelText("单实例叠层")]
        Stack,
    }
}