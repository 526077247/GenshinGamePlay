using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("球")]
    public class ConfigGearZoneSphere : ConfigGearZone
    {

        [SerializeField] public float radius;
    }
}