using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)][LabelText("一般对话框")]
    public partial class ConfigCommonDialogClip: ConfigStoryClip
    {
        [NinoMember(10)]
        public ConfigStoryText Text;
        [NinoMember(11)][LabelText("打字机效果")] 
        public bool Typewriter = true;
        [NinoMember(12)][LabelText("背景模糊")] 
        public bool BackgroundBlur;
        [NinoMember(13)][LabelText("等待点击再结束")]
        public bool WaitClick = true;
        [NinoMember(13)][LabelText("等待时间再结束")][ShowIf("@!"+nameof(WaitClick))]
        public int WaitTime = 1000;
        [NinoMember(14)][LabelText("播完后关闭窗口")] 
        public bool CloseOnOver;

        public override async ETTask Process(StorySystem storySystem)
        {
            var win = await UIManager.Instance.OpenWindow<UICommonStoryDialog, ConfigCommonDialogClip>(
                UICommonStoryDialog.PrefabPath, this);
            await win.Play();
            if (CloseOnOver)
            {
                await UIManager.Instance.CloseWindow(win);
            }
        }
    }
}