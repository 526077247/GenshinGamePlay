using System;
using ProtoBuf;
#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
#endif
namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigConditionByDataTrigger))]
    [ProtoInclude(101, typeof(ConfigConditionByStateTime))]
    [ProtoInclude(102, typeof(ConfigConditionByData))]
    public abstract partial class ConfigCondition
    {
        public abstract ConfigCondition Copy();
        public abstract bool IsMatch(Fsm fsm);
        public virtual void OnTransitionApply(Fsm fsm) { }

        public abstract bool Equals(ConfigCondition other);
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigConditionByData<bool>))]
    [ProtoInclude(101, typeof(ConfigConditionByData<float>))]
    [ProtoInclude(102, typeof(ConfigConditionByData<int>))]
    public abstract partial class ConfigConditionByData:ConfigCondition
    {
        [ProtoMember(1)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFSMConstKey)+"()", AppendNextDrawer = true)]
#endif
        public string Key;
        [ProtoMember(3)]
        public CompareMode Mode;
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigConditionByDataBool))]
    [ProtoInclude(101, typeof(ConfigConditionByDataFloat))]
    [ProtoInclude(102, typeof(ConfigConditionByDataInt))]
    public abstract partial class ConfigConditionByData<T> :ConfigConditionByData  where T : unmanaged
    {
        [ProtoMember(2)]
        public T Value;
    }
}