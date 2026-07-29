using System;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("增加变量的值")]
    [ProtoContract]
    public partial class ConfigSceneGroupAddVariableAction : ConfigSceneGroupAction
    {
        [JsonIgnore]
        public override bool CanSetOtherSceneGroup => true;
        [ProtoMember(10)]
        [LabelText("变量")]
        public string Key;
        [ProtoMember(11)]
        [LabelText("是否限制范围")]
        public bool Limit;
        [ProtoMember(12)]
        [ShowIf(nameof(Limit))] [LabelText("范围最小值")]
        public float MinValue;
        [ProtoMember(13)]
        [ShowIf(nameof(Limit))] [LabelText("范围最大值")]
        public float MaxValue;
        [ProtoMember(14)]
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
                    val = Mathf.Clamp(val, MinValue, MaxValue);
                }

                aimSceneGroup.Variable.Set(Key, val);
            }
        }
    }
}