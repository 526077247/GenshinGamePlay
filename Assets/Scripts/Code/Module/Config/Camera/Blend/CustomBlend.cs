using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class CustomBlend
    {
        [NinoMember(1)] public int from;
        [NinoMember(2)] public int to;
        [NinoMember(3)] public BlendDefinition definition;
        
    }
}