using System;
using Nino.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class VariableValue : AbstractVariableValue
    {
        [NinoMember(1)][LabelText("是否变量？")]
        public bool isDynamic;

        [NinoMember(2)][ShowIf("isDynamic")] [LabelText("变量")]
        public string key;

        [NinoMember(3)][ShowIf("@!isDynamic")] [LabelText("固定值")]
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