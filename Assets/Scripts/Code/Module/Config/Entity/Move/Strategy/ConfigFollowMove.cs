using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("跟随")]
    public partial class ConfigFollowMove: ConfigMoveStrategy
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