using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigRoute
    {
        public int localId;

        public ConfigPoint[] Points;
    }
}