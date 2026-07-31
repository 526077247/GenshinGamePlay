using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("串行执行")][ProtoContract]
    public partial class ConfigStorySerialClip: ConfigStoryClip
    {
        [ProtoMember(10)]
        public ConfigStoryClip[] Clips;
        
        public override async ETTask Process(StorySystem storySystem)
        {
            if (Clips != null)
            {
                for (int i = 0; i < Clips.Length; i++)
                {
                    await Clips[i].Process(storySystem);
                }
            }
        }
    }
}