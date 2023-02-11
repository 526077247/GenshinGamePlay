using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("通过ActorId释放实体")]
    public class ConfigGearReleaseEntityByActorIdAction:ConfigGearAction
    {
        [SerializeField]
        public int actorId;
    }
}