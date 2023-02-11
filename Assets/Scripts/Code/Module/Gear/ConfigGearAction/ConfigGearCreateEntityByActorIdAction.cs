using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("通过ActorId创建实体")]
    public class ConfigGearCreateEntityByActorIdAction : ConfigGearAction
    {
        [SerializeField]
        public int actorId;
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            aimGear.CreateActor(actorId);
        }
    }
}