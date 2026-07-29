using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 监听状态机状态
    /// </summary>
    [ProtoContract][LabelText("监听状态机状态变化时AttachModify")]
    public partial class ConfigAttachToStateIDMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
        public string ChargeLayer;
        [ProtoMember(2)]
        public List<string> StateIDs;
        [ProtoMember(3)]
        public ConfigAbilityPredicate Predicate;
        [ProtoMember(4)]
        public string ModifierName;
    }
}