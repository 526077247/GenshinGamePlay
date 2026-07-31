using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigStoryBranchClipItem
    {
        [ProtoMember(1)]
        public ConfigStoryText Text;
        [ProtoMember(2)]
        public ConfigStoryClip Clip;
    }
    
    [LabelText("选择分支执行")][ProtoContract]
    public partial class ConfigStoryBranchClip: ConfigStoryClip
    {
        [ProtoMember(10)][NotNull]
        public ConfigStoryBranchClipItem[] Branchs;

        public override async ETTask Process(StorySystem storySystem)
        {
            var win = await UIManager.Instance.OpenWindow<UIBranchStoryDialog, ConfigStoryBranchClip>(
                UIBranchStoryDialog.PrefabPath, this);
            var index = await win.WaitChoose();
            await UIManager.Instance.CloseWindow(win);
            if (Branchs[index].Clip != null)
                await Branchs[index].Clip.Process(storySystem);
            
        }
    }
}