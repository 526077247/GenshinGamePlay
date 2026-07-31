using System;
using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class SceneGroupValue : BaseSceneGroupValue
    {
        [ProtoMember(1)][LabelText("变量")]
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