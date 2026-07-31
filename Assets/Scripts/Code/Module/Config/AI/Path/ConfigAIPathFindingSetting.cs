using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public class ConfigAIPathFindingSetting
    {
        [ProtoMember(1)]
        public PathFindingType Type;
        [ProtoMember(2)][ShowIf(nameof(Type),PathFindingType.NavMesh)]
        public string NavMeshAgentName;
        [ProtoMember(3)][LabelText("开启动态避障")]
        public bool UseRVO2;
    }
}