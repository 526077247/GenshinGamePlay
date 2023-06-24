using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigStory
    {
        [NinoMember(1)]
        public ulong Id;
        
        [NinoMember(2)][LabelText("策划备注")]
        public string Remarks;
        
        [NinoMember(3)] 
        public ConfigStoryActor[] Actors;
            
        [NinoMember(4)][HideReferenceObjectPicker]
        public ConfigStorySerialClip Clips = new ();
        
    }
}