using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("增加变量的值")]
    public class ConfigGearAddVariableAction : ConfigGearAction
    {
        public override bool canSetOtherGear => true;
        
        [LabelText("变量")]
        [SerializeField] public string key;
        
        [SerializeField][LabelText("是否限制范围")]
        public bool limit;

        [ShowIf("limit")] [SerializeField][LabelText("范围最小值")]
        public float minValue;

        [ShowIf("limit")] [SerializeField][LabelText("范围最大值")]
        public float maxValue;
        [LabelText("增加的值")]
        [SerializeReference] public AbstractVariableValue value;
        
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            if (aimGear.variable != null)
            {
                float flag = value.Resolve(evt, aimGear.variable);

                var val = aimGear.variable.Get(key);
                val += flag;
                if (limit)
                {
                    val = (int) Mathf.Clamp(val, minValue, maxValue);
                }

                aimGear.variable.Set(key, val);
            }
        }
    }
}