using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigStory
    {
        [NinoMember(1)]
        public ulong Id;
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(3)] 
        public ConfigStoryActor[] Actors;
            
        [NinoMember(4)][HideReferenceObjectPicker]
        public ConfigStorySerialClip Clips = new ConfigStorySerialClip();
        
    }
}