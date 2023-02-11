using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("通过ActorId创建实体")]
    public class ConfigGearCreateEntityByActorIdAction : ConfigGearAction
    {
        [SerializeField]
        public int actorId;
    }
}