using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize][LabelText("监听状态机状态NormalizedTime变化时AttachModify")]
    public partial class ConfigAttachToNormalizedTimeMixin: ConfigAbilityMixin
    {
        [NinoMember(1)]
        public string ChargeLayer;
        [NinoMember(2)]
        public string StateID;
        [NinoMember(3)]
        public ConfigAbilityPredicate Predicate;
        [NinoMember(4)]
        public string ModifierName;
        [NinoMember(5)]
        public float normalizeStartRawNum;
        [NinoMember(6)]
        public float normalizeEndRawNum;
        public override AbilityMixin CreateAbilityMixin(ActorAbility actorAbility, ActorModifier actorModifier)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<AttachToStateIDMixin>.Type) as AttachToStateIDMixin;
            res.Init(actorAbility, actorModifier, this);
            return res;
        }
    }
}