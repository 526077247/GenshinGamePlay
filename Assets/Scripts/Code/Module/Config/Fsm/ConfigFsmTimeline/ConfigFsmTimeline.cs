using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFsmTimeline
    {
        public float length;
        public ConfigFsmClip[] clips;
    }
}