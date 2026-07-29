using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("并行执行")][ProtoContract]
    public partial class ConfigStoryParallelClip: ConfigStoryClip
    {
        [ProtoMember(10)]
        public ConfigStoryClip[] Clips;

        [ProtoMember(11, IsRequired = true)][LabelText("等待所有子项执行完成")]
        public bool WaitAll = true;

        public override async ETTask Process(StorySystem storySystem)
        {
            if (Clips != null)
            {
                using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
                {
                    for (int i = 0; i < Clips.Length; i++)
                    {
                        tasks.Add(Clips[i].Process(storySystem));
                    }
                    if (WaitAll)
                    {
                        await ETTaskHelper.WaitAll(tasks);
                    }
                    else
                    {
                        await ETTaskHelper.WaitAny(tasks);
                    }
                }
            }
        }
    }
}