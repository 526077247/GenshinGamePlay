using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigStoryBranchClipItem
    {
        [NinoMember(1)]
        public ConfigStoryText Text;
        [NinoMember(2)]
        public ConfigStoryClip Clip;
    }
    
    [LabelText("选择分支执行")][NinoSerialize]
    public partial class ConfigStoryBranchClip: ConfigStoryClip
    {
        [NinoMember(10)][NotNull]
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