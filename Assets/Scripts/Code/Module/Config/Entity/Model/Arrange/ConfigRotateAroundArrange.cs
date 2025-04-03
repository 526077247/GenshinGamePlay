using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("绕中心均匀旋转")]
    public partial class ConfigRotateAroundArrange: ConfigArrange
    {
       
        [NinoMember(11)][LabelText("角速度")]
        public BaseValue AngleSpeed = new ZeroValue();
        [NinoMember(12)][LabelText("半径")]
        public BaseValue Radius  = new SingleValue();
        [NinoMember(13)]
        public RotAngleType RotAngleType = RotAngleType.ROT_ANGLE_Y;
        [NinoMember(14)] [LabelText("跟随父物体(如果有)旋转")] 
        public bool FollowParentRotation;
    }
}