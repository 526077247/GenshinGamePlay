using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("球")]
    [NinoSerialize]
    public class ConfigGearZoneSphere : ConfigGearZone
    {
        [NinoMember(5)]
        public float radius;
        
        public override Zone CreateZone(Gear gear)
        {
            var entity = gear.Parent.CreateEntity<Zone>();
            entity.AddComponent<GearZoneComponent, int, long>(localId, gear.Id);
            return entity;
        }
    }
}