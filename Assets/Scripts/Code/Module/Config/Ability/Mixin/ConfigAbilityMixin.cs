using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigAttachToNormalizedTimeMixin))]
    [ProtoInclude(101, typeof(ConfigAttachToStateIDMixin))]
    [ProtoInclude(102, typeof(ConfigDoActionAfterAddMixin))]
    [ProtoInclude(103, typeof(ConfigDoActionAfterAttackMixin))]
    [ProtoInclude(104, typeof(ConfigDoActionAfterBeAttackMixin))]
    [ProtoInclude(105, typeof(ConfigDoActionAfterLoadObjectMixin))]
    [ProtoInclude(106, typeof(ConfigDoActionBeforeAttackMixin))]
    [ProtoInclude(107, typeof(ConfigDoActionBeforeBeAttackMixin))]
    [ProtoInclude(108, typeof(ConfigDoActionBeforeRemoveMixin))]
    [ProtoInclude(109, typeof(ConfigDoActionByExecuteMixin))]
    [ProtoInclude(110, typeof(ConfigDoActionByGadgetStateMixin))]
    [ProtoInclude(111, typeof(ConfigDoActionByStateIDMixin))]
    [ProtoInclude(112, typeof(ConfigDoActionByTickMixin))]
    [ProtoInclude(113, typeof(ConfigDoActionOnColliderBoxMixin))]
    [ProtoInclude(114, typeof(ConfigDoActionOnFsmTimelineTriggerMixin))]
    [ProtoInclude(115, typeof(ConfigDoActionOnInputMixin))]
    [ProtoInclude(116, typeof(ConfigDoActionOnInteeTouchMixin))]
    [ProtoInclude(117, typeof(ConfigDoActionOnTriggerMixin))]
    [ProtoInclude(118, typeof(ConfigDoActionOnUseSkillMixin))]
    public abstract partial class ConfigAbilityMixin
    {
    }
}