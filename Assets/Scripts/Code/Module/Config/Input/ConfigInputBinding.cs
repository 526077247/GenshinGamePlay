using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigInputBinding
    {
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(GameKeyCode)+"."+nameof(GameKeyCode.GetGameKeyCodeList)+"()")]
#endif
        [NinoMember(1)]
        public int GameBehavior;
        [NinoMember(2)]
        public KeyCode PC;
        [NinoMember(3)]
        public KeyCode Mobile;
    }
}