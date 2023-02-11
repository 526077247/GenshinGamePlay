using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("且 运算节点")]
    public class ConfigGearAndAction:ConfigGearAction
    {
        [SerializeReference][LabelText("条件")]
        [TypeFilter("@OdinDropdownHelper.GetFilteredConditionTypeList(handleType)")]
        public ConfigGearCondition[] conditions;
        [LabelText("所有条件都满足后执行")][OnCollectionChanged("Refresh")]
        [OnStateUpdate("Refresh")]
        [SerializeReference]
        public ConfigGearAction[] success;
        [LabelText("任意一个条件不满足执行")]
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