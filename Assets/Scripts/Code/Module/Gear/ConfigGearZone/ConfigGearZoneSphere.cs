using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("球")]
    public class ConfigGearZoneSphere : ConfigGearZone
    {

        [SerializeField] public float radius;
        
        public override Zone CreateZone(Gear gear)
        {
            var entity = gear.Parent.CreateEntity<Zone>();
            entity.AddComponent<GearZoneComponent, int, long>(localId, gear.Id);
            return entity;
        }
    }
}