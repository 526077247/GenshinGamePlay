using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigStory
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;

        [NinoMember(2)] 
        public ConfigStoryActor[] Actors;
            
        [NinoMember(3)][HideReferenceObjectPicker]
        public ConfigStorySerialClip Clips = new ();
        
        
    }
}