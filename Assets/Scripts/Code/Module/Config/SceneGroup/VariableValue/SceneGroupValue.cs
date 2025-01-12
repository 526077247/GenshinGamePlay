using System;
using Nino.Core;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class SceneGroupValue : BaseSceneGroupValue
    {
        [NinoMember(1)][LabelText("变量")]
        public string Key;
        
        public override float Resolve(IEventBase obj, DynDictionary set)
        {
            if (set.TryGet(Key, out float f))
            {
                return f;
            }
            return default;
        }
    }
}