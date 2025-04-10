using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigAIPathFindingSetting
    {
        [NinoMember(1)]
        public PathFindingType Type;
        [NinoMember(2)][ShowIf(nameof(Type),PathFindingType.NavMesh)]
        public string NavMeshAgentName;
        [NinoMember(3)][LabelText("开启动态避障")]
        public bool UseRVO2;
    }
}