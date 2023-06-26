using Nino.Serialization;

namespace TaoTie
{

    public abstract partial class ConfigStoryTimeLineClip
    {
        [NinoMember(1)]
        public float StartTime = 0;

        public abstract void Process(float timeNow, StoryTimeLineRunner runner);
    }
}