﻿using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigEquipController
    {
        [NinoMember(1)]
        public Dictionary<EquipType, string> AttachPoints;
        [NinoMember(2)]
        public string SheathPoint;
    }
}