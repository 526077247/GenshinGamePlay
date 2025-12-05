using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaoTie
{
    public class UIInputTextmesh:UIBaseContainer,IOnDestroy,IOnCreate
    {
#if UNITY_WEBGL
        private bool isShowKeyboard = false;
#endif
        private TMPro.TMP_InputField input;
        
        private UnityAction<string> onValueChange;

        private UnityAction<string> onEndEdit;

        public void OnCreate()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ActivatingComponent();
            this.input.onSelect.AddListener(OnSelect);
#if MINIGAME_SUBPLATFORM_KUAISHOU || MINIGAME_SUBPLATFORM_WEIXIN || MINIGAME_SUBPLATFORM_DOUYIN
            this.input.onDeselect.AddListener(OnDeselect);
#endif
#endif
        }
        public void OnDestroy()
        {
            this.RemoveOnValueChanged();
            this.RemoveOnEndEdit();
#if UNITY_WEBGL && !UNITY_EDITOR
            this.input.onSelect.RemoveListener(OnSelect);
#if MINIGAME_SUBPLATFORM_KUAISHOU || MINIGAME_SUBPLATFORM_WEIXIN || MINIGAME_SUBPLATFORM_DOUYIN
            this.input.onDeselect.RemoveListener(OnDeselect);
#endif
#endif
        }
        #region WebGL支持
#if UNITY_WEBGL && !UNITY_EDITOR
        private void OnSelect(string text)
        {
            if (isShowKeyboard)
                return;
            isShowKeyboard = true;
#if MINIGAME_SUBPLATFORM_WEIXIN
            WeChatWASM.WX.ShowKeyboard(new ()
                {
                    defaultValue = input.text,
                    maxLength = input.characterLimit <= 0 ? 9999 : input.characterLimit,
                    confirmType = "done",
                }
            );
            WeChatWASM.WX.OnKeyboardConfirm(this.OnConfirm);
            WeChatWASM.WX.OnKeyboardComplete(this.OnComplete);
            WeChatWASM.WX.OnKeyboardInput(this.OnInput);
#elif MINIGAME_SUBPLATFORM_KUAISHOU
            KSWASM.KS.ShowKeyboard(new ()
                {
                    defaultValue = input.text,
                    maxLength = input.characterLimit <= 0 ? 9999 : input.characterLimit,
                    confirmType = "done",
                }
            );
            KSWASM.KS.OnKeyboardConfirm(this.OnConfirm);
            KSWASM.KS.OnKeyboardComplete(this.OnComplete);
            KSWASM.KS.OnKeyboardInput(this.OnInput);
#elif MINIGAME_SUBPLATFORM_DOUYIN
            TTSDK.TT.ShowKeyboard(new ()
                {
                    defaultValue = input.text,
                    maxLength = input.characterLimit <= 0 ? 9999 : input.characterLimit,
                    confirmType = "done",
                }
            );
            TTSDK.TT.OnKeyboardConfirm += this.OnConfirm;
            TTSDK.TT.OnKeyboardComplete += this.OnComplete;
            TTSDK.TT.OnKeyboardInput += this.OnInput;
#else
            if(PlatformUtil.IsHuaWeiGroup() || !PlatformUtil.IsMobile())
            {
                return;
            }
            BridgeHelper.SetUpOverlayDialog((input.placeholder is TMPro.TMP_Text a)?a.text:"", this.input.text,
                I18NManager.Instance.I18NGetText(I18NKey.Global_Btn_Confirm),
                I18NManager.Instance.I18NGetText(I18NKey.Global_Btn_Cancel));
            OverlayHtmlCoroutine().Coroutine();
#endif
        }

        private void OnDeselect(string text)
        {
            HideKeyboard();
        }
 private void HideKeyboard()
        {
            if (!isShowKeyboard)
                return;
            isShowKeyboard = false;
#if MINIGAME_SUBPLATFORM_WEIXIN
            WeChatWASM.WX.HideKeyboard(new ());
            WeChatWASM.WX.OffKeyboardInput(OnInput);
            WeChatWASM.WX.OffKeyboardConfirm(OnConfirm);
            WeChatWASM.WX.OffKeyboardComplete(OnComplete);
#elif MINIGAME_SUBPLATFORM_KUAISHOU
            KSWASM.KS.HideKeyboard(new ());
            KSWASM.KS.OffKeyboardInput(this.OnInput);
            KSWASM.KS.OffKeyboardConfirm(this.OnConfirm);
            KSWASM.KS.OffKeyboardComplete(this.OnComplete);
#elif MINIGAME_SUBPLATFORM_DOUYIN
            TTSDK.TT.HideKeyboard();
            TTSDK.TT.OnKeyboardConfirm -= this.OnConfirm;
            TTSDK.TT.OnKeyboardComplete -= this.OnComplete;
            TTSDK.TT.OnKeyboardInput -= this.OnInput;
#endif
        }
#if MINIGAME_SUBPLATFORM_WEIXIN
        private void OnInput(WeChatWASM.OnKeyboardInputListenerResult v)
        {
            if (input.isFocused)
            {
                input.text = v.value;
            }
        }
        private void OnConfirm(WeChatWASM.OnKeyboardInputListenerResult v)
        {
            HideKeyboard();
        }
        private void OnComplete(WeChatWASM.OnKeyboardInputListenerResult v)
        {
            HideKeyboard();
        }
#elif MINIGAME_SUBPLATFORM_KUAISHOU
        private void OnInput(KSWASM.OnKeyboardInputListenerResult v)
        {
            if (input.isFocused)
            {
                input.text = v.value;
            }
        }
        private void OnConfirm(KSWASM.OnKeyboardInputListenerResult v)
        {
            HideKeyboard();
        }
        private void OnComplete(KSWASM.OnKeyboardInputListenerResult v)
        {
            HideKeyboard();
        }
#elif MINIGAME_SUBPLATFORM_DOUYIN
        private void OnInput(string v)
        {
            if (input.isFocused)
            {
                input.text = v;
            }
        }
        private void OnConfirm(string v)
        {
            HideKeyboard();
        }
        private void OnComplete(string v)
        {
            HideKeyboard();
        }
#else

        private async ETTask OverlayHtmlCoroutine()
        {
            await UnityLifeTimeHelper.WaitFrameFinish();
            input.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
            WebGLInput.captureAllKeyboardInput = false;
            while (BridgeHelper.IsOverlayDialogActive())
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            WebGLInput.captureAllKeyboardInput = true;

            if (!BridgeHelper.IsOverlayDialogCanceled())
            {
                input.text = BridgeHelper.GetOverlayDialogValue();
            }
            HideKeyboard();
        }
#endif
#endif
        #endregion
        void ActivatingComponent()
        {
            if (this.input == null)
            {
                this.input = this.GetGameObject().GetComponent<TMPro.TMP_InputField>();
                if (this.input == null)
                {
                    Log.Error($"添加UI侧组件UIInputTextmesh时，物体{this.GetGameObject().name}上没有找到TMPro.TMP_InputField组件");
                }
            }
        }
        public string GetText()
        {
            this.ActivatingComponent();
            return this.input.text;
        }

        public void SetText(string text)
        {
            this.ActivatingComponent();
            this.input.text = text;
        }

        public void SetOnValueChanged(Action func)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.onValueChange = (a) =>
            {
                func?.Invoke();
            };
            this.input.onValueChanged.AddListener(this.onValueChange);
        }

        public void RemoveOnValueChanged()
        {
            if(this.onValueChange!=null)
                this.input.onValueChanged.RemoveListener(this.onValueChange);
            this.onValueChange = null;
        }
        
        
        public void SetOnEndEdit(Action func)
        {
            this.ActivatingComponent();
            this.RemoveOnEndEdit();
            this.onEndEdit = (a) =>
            {
                func?.Invoke();
            };
            this.input.onEndEdit.AddListener(this.onEndEdit);
        }
        
        public void RemoveOnEndEdit()
        {
            if(this.onEndEdit!=null)
                this.input.onEndEdit.RemoveListener(this.onEndEdit);
            this.onEndEdit = null;
        }
        
        public void SetInteractable(bool flag)
        {
            this.ActivatingComponent();
            this.input.interactable = flag;
        }
    }
}