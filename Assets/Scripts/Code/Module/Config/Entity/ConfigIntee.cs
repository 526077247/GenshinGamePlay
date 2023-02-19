using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 交互面板配置
    /// </summary>
    [NinoSerialize]
    public partial class ConfigIntee
    {
        [NinoMember(1)]
        public float Radius;
        [NinoMember(2)]
        public float Height;
        [NinoMember(3)]
        public int CheckInterval;
        [NinoMember(4)] 
        public Dictionary<int, string[]> Params;
    }
}