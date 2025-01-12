using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    
    [NinoType(false)]
    public class ConfigBillboardHpPlugin: ConfigBillboardPlugin
    {
        [NinoMember(10)]
        public Color BleedColor = Color.green;
        [NinoMember(11)]
        public Color BgColor = Color.grey;
        [NinoMember(12)]
        public Vector2 Size = new Vector2(200, 10);
    }
}