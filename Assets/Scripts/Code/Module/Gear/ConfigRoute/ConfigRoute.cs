using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigRoute
    {
        public int localId;

        public ConfigPoint[] Points;
    }
}