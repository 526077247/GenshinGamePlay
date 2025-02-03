using System;
using DaGenGraph;
using Nino.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    [LabelText("且 运算节点")]
    [NinoType(false)]
    public partial class ConfigSceneGroupAndAction:ConfigSceneGroupLogicConditionAction
    {
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            bool isSuc = true;
            if (Conditions == null || Conditions.Length == 0)
            {
                isSuc = true;
            }
            else
            {
                for (int i = 0; i < Conditions.Length; i++)
                {
                    isSuc &= Conditions[i].IsMatch(evt,aimSceneGroup);
                }
            }

            if (isSuc)
            {
                for (int i = 0; i < (Success == null ? 0 : Success.Length); i++)
                {
                    Success[i]?.ExecuteAction(evt,aimSceneGroup,fromSceneGroup);
                }
            }
            else
            {
                for (int i = 0; i < (Fail==null?0:Fail.Length); i++)
                {
                    Fail[i]?.ExecuteAction(evt,aimSceneGroup,fromSceneGroup);
                }
            }
        }
    }
}