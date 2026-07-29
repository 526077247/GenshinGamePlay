using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract][LabelText("跟随")]
    public partial class ConfigFollowMove: ConfigMoveStrategy
    {
        [ProtoMember(11)]
        public bool FollowRotation;
        [ProtoMember(12)]
        public Vector3 Offset;
        [ProtoMember(13)] 
        public bool FollowOwnerInvisible;
        [ProtoMember(14)] 
        public bool ForceFaceToTarget;
        [ProtoMember(15)] 
        public bool DestroyOnTargetDispose;
    }
}