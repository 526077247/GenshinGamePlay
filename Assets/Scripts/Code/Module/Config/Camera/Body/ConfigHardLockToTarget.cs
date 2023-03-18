using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigHardLockToTarget
    {
        [NinoMember(1)]
        public float damping = 0;
        
    }
}