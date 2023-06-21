using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public abstract class ConfigStoryClip
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;


        public abstract ETTask Process();
    }
}