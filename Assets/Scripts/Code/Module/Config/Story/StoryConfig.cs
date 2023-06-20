using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class StoryConfig
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;

        [NinoMember(2)] 
        public StoryActorConfig[] Actors;
            
        [NinoMember(3)][HideReferenceObjectPicker]
        public SerialStoryClipConfig Clips = new ();
        
        
    }
}