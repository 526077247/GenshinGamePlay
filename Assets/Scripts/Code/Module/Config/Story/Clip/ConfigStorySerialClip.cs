using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("串行执行")][NinoSerialize]
    public class ConfigStorySerialClip: ConfigStoryClip
    {
        [NinoMember(10)]
        public ConfigStoryClip[] Clips;
        
        public override async ETTask Process()
        {
            if (Clips != null)
            {
                for (int i = 0; i < Clips.Length; i++)
                {
                    await Clips[i].Process();
                }
            }
        }
    }
}