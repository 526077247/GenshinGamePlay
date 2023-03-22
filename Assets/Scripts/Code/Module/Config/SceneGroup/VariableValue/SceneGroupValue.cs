using System;
using Nino.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SceneGroupValue : BaseSceneGroupValue
    {
        [NinoMember(1)][LabelText("变量")]
        public string key;
        
        public override float Resolve(IEventBase obj, DynDictionary set)
        {
            if (set.TryGet(key, out float f))
            {
                return f;
            }
            return default;
        }
    }
}