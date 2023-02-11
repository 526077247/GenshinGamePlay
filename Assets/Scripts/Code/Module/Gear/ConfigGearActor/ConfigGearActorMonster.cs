using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable]
    public sealed class ConfigGearActorMonster : ConfigGearActor
    {
        [SerializeField] public int configID;
        [SerializeField] public int AIId;
        
        public override Entity CreateActor(Gear gear)
        {
            var entity = gear.Parent.CreateEntity<Monster, int>(configID);
            entity.AddComponent<GearActorComponent, int, long>(localId, gear.Id);
            return entity;
        }
    }
}