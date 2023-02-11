using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace TaoTie
{
    [LabelText("立方")]
    [NinoSerialize]
    public partial class ConfigGearZoneOBB : ConfigGearZone
    {
        [NinoMember(5)]
        public Vector3 rotation;
        [NinoMember(6)]
        public Vector3 size;

        public override Zone CreateZone(Gear gear)
        {
            var entity = gear.Parent.CreateEntity<Zone>();
            entity.AddComponent<GearZoneComponent, int, long>(localId, gear.Id);
            return entity;
        }
    }
}