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
    }
}