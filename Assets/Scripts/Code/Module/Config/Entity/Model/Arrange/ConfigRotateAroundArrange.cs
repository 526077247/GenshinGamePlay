using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract][LabelText("绕中心均匀旋转")]
    public partial class ConfigRotateAroundArrange: ConfigArrange
    {
       
        [ProtoMember(11, IsRequired = true)][LabelText("角速度")]
        public BaseValue AngleSpeed = new ZeroValue();
        [ProtoMember(12, IsRequired = true)][LabelText("半径")]
        public BaseValue Radius  = new SingleValue();
        [ProtoMember(13, IsRequired = true)]
        public RotAngleType RotAngleType = RotAngleType.ROT_ANGLE_Y;
        [ProtoMember(14)] [LabelText("跟随父物体(如果有)旋转")] 
        public bool FollowParentRotation;
    }
}