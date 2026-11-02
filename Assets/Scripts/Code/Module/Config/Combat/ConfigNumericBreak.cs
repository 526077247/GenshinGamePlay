using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 一条削韧配置：某打击部位 对 某数值属性 削多少。
    ///
    /// 为什么是「一条 = 部位 + 属性 + 值」的扁平三元组，而不是
    /// <c>Dictionary&lt;HitBoxType, BaseValue&gt;</c> 或嵌套字典：
    /// 1. 需要同时表达「不同部位」和「不同属性」，二维必须落到两个字段上；
    /// 2. protobuf 的 map value 不能是 map，字典套字典只能再套一层壳类，
    ///    编辑时要点开三层，配置一多就没人愿意改；
    /// 3. 扁平列表在编辑器里就是一张三列表格，加一种组合＝加一行。
    ///
    /// <see cref="NumericKey"/> 填的是 <see cref="NumericType.Map"/> 里的组首名（"EN"），
    /// 而不是段位（1012 / 10121）。原因见 <c>AttackHelper.ApplyNumericBreak</c>：
    /// 组首名唯一，填段位会出现「1012 和 10121 都能写」的歧义，写错段位等于改上限。
    /// </summary>
    [ProtoContract]
    public partial class ConfigNumericBreak
    {
        [ProtoMember(1)][LabelText("*打击部位")]
        [Tooltip("只对命中该部位的攻击生效。同一属性想对多个部位都削，就各写一条")]
        public HitBoxType HitBox = HitBoxType.Normal;

        [ProtoMember(2)][LabelText("*属性名")]
        [Tooltip("填属性表的 Key 列（组首名），如 EN / Shield。不要填 ENBase、MaxEN 这类段位或上限属性")]
        public string NumericKey = "EN";

        [ProtoMember(3)][LabelText("*削韧值")][NotNull]
        [Tooltip("可写公式或数值引用，结算时原样参与计算。整数属性写入时四舍五入，小数属性保留精度")]
        public BaseValue Value = new SingleValue();
    }
}
