using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAIPathFindingSetting
    {
        [NinoMember(1)]
        public PathFindingType Type;
        [NinoMember(2)][ShowIf("@Type == PathFindingType.NavMesh")]
        public string NavMeshAgentName;
    }
}