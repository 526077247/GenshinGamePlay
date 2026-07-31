using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("监听状态机状态NormalizedTime变化时AttachModify")]
    public partial class ConfigAttachToNormalizedTimeMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)]
        public string ChargeLayer;
        [ProtoMember(2)]
        public string StateID;
        [ProtoMember(3)]
        public ConfigAbilityPredicate Predicate;
        [ProtoMember(4)]
        public string ModifierName;
        [ProtoMember(5)]
        public float normalizeStartRawNum;
        [ProtoMember(6)]
        public float normalizeEndRawNum;
    }
}