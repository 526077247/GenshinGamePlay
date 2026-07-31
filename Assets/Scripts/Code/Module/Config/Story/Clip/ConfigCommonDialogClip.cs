using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract][LabelText("一般对话框")]
    public partial class ConfigCommonDialogClip: ConfigStoryClip
    {
        [ProtoMember(10)]
        public ConfigStoryText Text;
        [ProtoMember(11, IsRequired = true)][LabelText("打字机效果")] 
        public bool Typewriter = true;
        [ProtoMember(12)][LabelText("背景模糊")] 
        public bool BackgroundBlur;
        [ProtoMember(13, IsRequired = true)][LabelText("等待点击再结束")]
        public bool WaitClick = true;
        [ProtoMember(14, IsRequired = true)][LabelText("等待时间再结束")][ShowIf("@!"+nameof(WaitClick))]
        public int WaitTime = 1000;
        [ProtoMember(15)][LabelText("播完后关闭窗口")] 
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