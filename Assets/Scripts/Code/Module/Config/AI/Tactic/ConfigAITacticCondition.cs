using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAITacticCondition
    {
        [ProtoMember(1)][Tooltip("处于这些Pose中时有效, 为null表示全有效")]
        public int[] PoseId;
    }
}