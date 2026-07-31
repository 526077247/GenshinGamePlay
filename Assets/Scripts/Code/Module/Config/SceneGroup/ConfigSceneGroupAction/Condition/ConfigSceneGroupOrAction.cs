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
    [LabelText("与 逻辑节点")]
    [ProtoContract]
    public partial class ConfigSceneGroupOrAction : ConfigSceneGroupLogicConditionAction
    {
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            bool isSuc = false;
            if (Conditions == null || Conditions.Length == 0)
            {
                isSuc = false;
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