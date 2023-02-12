using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigHitScene
    {
        [NinoMember(1)]
        public string DefaultEffect;
        [NinoMember(2)]
        public Dictionary<string, string> SurfaceEffect;
    }
}