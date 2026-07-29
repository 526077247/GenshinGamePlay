using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract][LabelText("寻路")]
    public partial class ConfigPlatformMove: ConfigMoveStrategy
    {
        [ProtoMember(10)]
        public ConfigRoute Route;
        [ProtoMember(11, IsRequired = true)][LabelText("*延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")][ShowIf("@"+nameof(Route)+"!=null")]
        public int Delay = -1;
    }
}