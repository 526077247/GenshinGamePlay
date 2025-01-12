using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigEquipController
    {
        [NinoMember(1)]
        public Dictionary<EquipType, string> AttachPoints = new();
        [NinoMember(2)]
        public string SheathPoint;
    }
}