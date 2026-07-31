using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigInputBinding
    {
#if UNITY_EDITOR 
        [ValueDropdown("@"+nameof(GameKeyCode)+"."+nameof(GameKeyCode.GetGameKeyCodeList)+"()")]
#endif
        [ProtoMember(1)]
        public int GameBehavior;
        [ProtoMember(2)]
        public KeyCode PC;
        [ProtoMember(3)]
        public KeyCode Mobile;
    }
}