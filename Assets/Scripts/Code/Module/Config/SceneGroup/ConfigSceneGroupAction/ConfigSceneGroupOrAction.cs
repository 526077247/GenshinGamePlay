using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    [LabelText("与 逻辑节点")]
    [NinoSerialize]
    public partial class ConfigSceneGroupOrAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        [LabelText("条件")]
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredConditionTypeList)+"("+nameof(HandleType)+")")]
        public ConfigSceneGroupCondition[] Conditions;
        [NinoMember(11)]
        [LabelText("满足任意一个条件后执行")]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))]
        [OnStateUpdate(nameof(Refresh))]
#endif
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredActionTypeList)+"("+nameof(HandleType)+")")]
        public ConfigSceneGroupAction[] Success;
        [NinoMember(12)]
        [LabelText("所有条件都不满足后执行")]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))]
        [OnStateUpdate(nameof(Refresh))]
#endif
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredActionTypeList)+"("+nameof(HandleType)+")")]
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
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            bool isSuc = false;
            if (Conditions == null || Conditions.Length == 0)
            {
                isSuc = true;
            }
            else
            {
                for (int i = 0; i < Conditions.Length; i++)
                {
                    isSuc |= Conditions[i].IsMatch(evt, aimSceneGroup);
                }
            }

            if (isSuc)
            {
                for (int i = 0; i < (Success == null ? 0 : Success.Length); i++)
                {
                    Success[i]?.ExecuteAction(evt, aimSceneGroup,fromSceneGroup);
                }
            }
            else
            {
                for (int i = 0; i < (Fail == null ? 0 : Fail.Length); i++)
                {
                    Fail[i]?.ExecuteAction(evt, aimSceneGroup,fromSceneGroup);
                }
            }
        }
    }
}