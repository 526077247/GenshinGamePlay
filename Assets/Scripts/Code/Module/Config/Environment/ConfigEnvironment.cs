using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigEnvironment
    {
#if UNITY_EDITOR
        [NinoMember(0)][Sirenix.OdinInspector.LabelText("策划备注")]
        public int Remarks;
#endif
        [NinoMember(1)]
        public int Id;
        [NinoMember(2)]
        public ConfigBlender Enter;
        [NinoMember(3)]
        public ConfigBlender Leave;
    }
}