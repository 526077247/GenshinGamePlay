using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigFollowMove: ConfigMoveAgent
    {
        [NinoMember(11)]
        public bool FollowRotation;
        [NinoMember(12)]
        public Vector3 Offset;
        [NinoMember(13)] 
        public bool FollowOwnerInvisible;
        [NinoMember(14)] 
        public bool ForceFaceToTarget;
        [NinoMember(15)] 
        public bool DestroyOnTargetDispose;
    }
}