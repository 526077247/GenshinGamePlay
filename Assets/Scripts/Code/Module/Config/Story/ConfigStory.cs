using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigStory
    {
        [ProtoMember(1)]
        public ulong Id;
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(3)] 
        public ConfigStoryActor[] Actors;
            
        [ProtoMember(4, IsRequired = true)][HideReferenceObjectPicker]
        public ConfigStorySerialClip Clips = new ConfigStorySerialClip();
        
    }
}