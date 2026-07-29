using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract][LabelText("输入状态修改")]
    public partial class ConfigStoryChangeInputState: ConfigStoryClip
    {
        [ProtoMember(10)][LabelText("开启输入状态")]
        public bool Active;
        
        [ProtoMember(11)][LabelText("修改光标状态")]
        public bool EffectCursor;
        
        [ProtoMember(12, IsRequired = true)][LabelText("光标是否不锁定")][ShowIf(nameof(EffectCursor))]
        public bool UnLockCursor = true;

        [ProtoMember(13, IsRequired = true)][LabelText("显示光标")][ShowIf(nameof(EffectCursor))]
        public bool VisibleCursor = true;

        public override async ETTask Process(StorySystem storySystem)
        {
            if (EffectCursor)
            {
                CameraManager.Instance.ChangeCursorVisible(VisibleCursor, CursorStateType.Story);
                CameraManager.Instance.ChangeCursorLock(UnLockCursor, CursorStateType.Story);
            }
            else
            {
                CameraManager.Instance.ChangeCursorVisible(false, CursorStateType.Story);
                CameraManager.Instance.ChangeCursorLock(false, CursorStateType.Story);
            }

            if (Active)
            {
                await TimerManager.Instance.WaitAsync(500);
            }
            InputManager.Instance.IsPause = !Active;
        }
    }
}