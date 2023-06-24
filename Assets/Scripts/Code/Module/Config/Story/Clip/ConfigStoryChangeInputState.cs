using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize][LabelText("输入状态修改")]
    public class ConfigStoryChangeInputState: ConfigStoryClip
    {
        [NinoMember(10)][LabelText("开启输入状态")]
        public bool Active;
        
        [NinoMember(11)][LabelText("修改光标状态")]
        public bool EffectCursor;
        
        [NinoMember(12)][LabelText("光标锁定模式")][ShowIf(nameof(EffectCursor))]
        public CursorLockMode Mode = CursorLockMode.None;

        [NinoMember(13)][LabelText("显示光标")][ShowIf(nameof(EffectCursor))]
        public bool VisibleCursor = true;

        public override async ETTask Process()
        {
            if (EffectCursor)
            {
                CameraManager.Instance.ChangeCursorState(Mode,VisibleCursor);
                
            }
            else
            {
                CameraManager.Instance.ResetCursorState();
            }

            if (Active)
            {
                await TimerManager.Instance.WaitAsync(500);
            }
            InputManager.Instance.IsPause = !Active;
        }
    }
}