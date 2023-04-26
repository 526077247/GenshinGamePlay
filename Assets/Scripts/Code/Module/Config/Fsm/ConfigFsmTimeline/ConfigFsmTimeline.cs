using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFsmTimeline
    {
        public float Length;
        public ConfigFsmClip[] Clips;
    }
}