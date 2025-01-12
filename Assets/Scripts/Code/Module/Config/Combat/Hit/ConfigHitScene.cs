using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigHitScene
    {
        [NinoMember(1)]
        public string DefaultEffect;
        [NinoMember(2)]
        public Dictionary<string, string> SurfaceEffect = new();
    }
}