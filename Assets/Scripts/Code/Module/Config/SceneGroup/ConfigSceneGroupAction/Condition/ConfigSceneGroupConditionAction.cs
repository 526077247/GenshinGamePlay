using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;
namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigSceneGroupNormalConditionAction))]
    [ProtoInclude(101, typeof(ConfigSceneGroupLogicConditionAction))]
    public abstract class ConfigSceneGroupConditionAction:ConfigSceneGroupAction
    {
        [ProtoMember(11)]
        [LabelText("满足条件后执行")][Inspector.DrawIgnore(Inspector.Ignore.Graph)]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))]
        [OnStateUpdate(nameof(Refresh))]
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredActionTypeList)+"("+nameof(HandleType)+")")]
#endif
        public ConfigSceneGroupAction[] Success;
        [ProtoMember(12)]
        [LabelText("不满足后执行")][Inspector.DrawIgnore(Inspector.Ignore.Graph)]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))]
        [OnStateUpdate(nameof(Refresh))]
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredActionTypeList)+"("+nameof(HandleType)+")")]
#endif
        public ConfigSceneGroupAction[] Fail;
#if UNITY_EDITOR
        
        private void Refresh()
        {
            if (Success!= null)
            {
                for (int i = 0; i <  Success.Length; i++)
                {
                    if(Success[i]!=null)
                        Success[i].HandleType = HandleType;
                }
                Success.Sort((a, b) => { return a.LocalId - b.LocalId;});
            }

            if (Fail != null)
            {
                for (int i = 0; i <  Fail.Length; i++)
                {
                    if(Fail[i]!=null)
                        Fail[i].HandleType = HandleType;
                }
                Fail.Sort((a, b) => { return a.LocalId - b.LocalId;});
            }
        }
#endif
    }
}