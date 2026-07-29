using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public abstract partial class ConfigStoryTimeLineClip
    {
        [ProtoMember(1, IsRequired = true)]
        public float StartTime = 0;

        public abstract void Process(float timeNow, StoryTimeLineRunner runner);
    }
}