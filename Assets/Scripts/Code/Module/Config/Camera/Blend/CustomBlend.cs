using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class CustomBlend
    {
        [NinoMember(1)] public int From;
        [NinoMember(2)] public int To;
        [NinoMember(3)] public BlendDefinition Definition;
        
    }
}