using System;

using ProtoBuf;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigSceneGroupAndAction))]
    [ProtoInclude(101, typeof(ConfigSceneGroupOrAction))]
    public abstract class ConfigSceneGroupLogicConditionAction:ConfigSceneGroupConditionAction
    {
        [ProtoMember(10)]
        [LabelText("条件")][Inspector.DrawIgnore]
#if UNITY_EDITOR
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredConditionTypeList)+"("+nameof(HandleType)+")")]
#endif
        public ConfigSceneGroupCondition[] Conditions;
    }
}