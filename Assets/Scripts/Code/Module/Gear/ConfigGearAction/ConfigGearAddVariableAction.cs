using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("增加变量的值")]
    [NinoSerialize]
    public class ConfigGearAddVariableAction : ConfigGearAction
    {
        [JsonIgnore]
        public override bool canSetOtherGear => true;
        [NinoMember(10)]
        [LabelText("变量")]
        public string key;
        [NinoMember(11)]
        [LabelText("是否限制范围")]
        public bool limit;
        [NinoMember(12)]
        [ShowIf(nameof(limit))] [LabelText("范围最小值")]
        public float minValue;
        [NinoMember(13)]
        [ShowIf(nameof(limit))] [LabelText("范围最大值")]
        public float maxValue;
        [NinoMember(14)]
        [LabelText("增加的值")]
        public AbstractVariableValue value;
        
        
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