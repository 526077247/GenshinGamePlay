using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigCommonDialogClip))]
    [ProtoInclude(101, typeof(ConfigStoryBranchClip))]
    [ProtoInclude(102, typeof(ConfigStoryChangeInputState))]
    [ProtoInclude(103, typeof(ConfigStoryParallelClip))]
    [ProtoInclude(104, typeof(ConfigStorySerialClip))]
    [ProtoInclude(105, typeof(ConfigStoryWaitTimeClip))]
    [ProtoInclude(106, typeof(ConfigStoryTimeLine))]
    public abstract partial class ConfigStoryClip
    {
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif

        public abstract ETTask Process(StorySystem storySystem);
    }
}