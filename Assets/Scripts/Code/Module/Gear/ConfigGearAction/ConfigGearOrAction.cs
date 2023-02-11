using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    [Serializable]
    [LabelText("与 逻辑节点")]
    public class ConfigGearOrAction : ConfigGearAction
    {

        [SerializeReference][LabelText("条件")]
        [TypeFilter("@OdinDropdownHelper.GetFilteredConditionTypeList(handleType)")]
        public ConfigGearCondition[] conditions;

        [LabelText("满足任意一个条件后执行")]
        [SerializeReference][OnCollectionChanged("Refresh")]
        [OnStateUpdate("Refresh")]
        public ConfigGearAction[] success;

        [LabelText("所有条件都不满足后执行")]
        [SerializeReference][OnCollectionChanged("Refresh")]
        [OnStateUpdate("Refresh")]
        public ConfigGearAction[] fail;
#if UNITY_EDITOR
        
        public void Refresh()
        {
            if (success!= null)
            {
                for (int i = 0; i <  success.Length; i++)
                {
                    if(success[i]!=null)
                        success[i].handleType = handleType;
                }
                success.Sort((a, b) => { return a.localId - b.localId;});
            }

            if (fail != null)
            {
                for (int i = 0; i <  fail.Length; i++)
                {
                    if(fail[i]!=null)
                        fail[i].handleType = handleType;
                }
                fail.Sort((a, b) => { return a.localId - b.localId;});
            }
        }
#endif
    }
}