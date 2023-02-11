using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [Serializable]
    public class VariableValue : AbstractVariableValue
    {
        [SerializeField] [LabelText("是否变量？")]
        public bool isDynamic;

        [ShowIf("isDynamic")] [SerializeField][LabelText("变量")]
        public string key;

        [ShowIf("@!isDynamic")] [SerializeField] [LabelText("固定值")]
        public int fixedValue;
        
        public override float Resolve(IEventBase obj, VariableSet set)
        {
            if (!isDynamic)
            {
                return fixedValue;
            }

            if (set.TryGet(key, out float f))
            {
                return f;
            }
            return default;
        }
    }
}