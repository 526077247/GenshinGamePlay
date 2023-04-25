using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("增加变量的值")]
    [NinoSerialize]
    public partial class ConfigSceneGroupAddVariableAction : ConfigSceneGroupAction
    {
        [JsonIgnore]
        public override bool CanSetOtherSceneGroup => true;
        [NinoMember(10)]
        [LabelText("变量")]
        public string Key;
        [NinoMember(11)]
        [LabelText("是否限制范围")]
        public bool Limit;
        [NinoMember(12)]
        [ShowIf(nameof(Limit))] [LabelText("范围最小值")]
        public float MinValue;
        [NinoMember(13)]
        [ShowIf(nameof(Limit))] [LabelText("范围最大值")]
        public float MaxValue;
        [NinoMember(14)]
        [LabelText("增加的值")]
        public BaseSceneGroupValue Value;
        
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (aimSceneGroup.Variable != null)
            {
                float flag = Value.Resolve(evt, aimSceneGroup.Variable);

                var val = aimSceneGroup.Variable.Get(Key);
                val += flag;
                if (Limit)
                {
                    val = (int) Mathf.Clamp(val, MinValue, MaxValue);
                }

                aimSceneGroup.Variable.Set(Key, val);
            }
        }
    }
}