using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaoTie
{
    public class UIInputTextmesh:UIBaseContainer,IOnDestroy,IOnCreate
    {
#if UNITY_WEBGL
        public bool UseDialog = false;
#endif
        private TMPro.TMP_InputField input;
        
        private UnityAction<string> onValueChange;

        private UnityAction<string> onEndEdit;

        public void OnCreate()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ActivatingComponent();
            this.input.onSelect.AddListener(OnSelect);
#endif
        }
        public void OnDestroy()
        {
            this.RemoveOnValueChanged();
            this.RemoveOnEndEdit();
#if UNITY_WEBGL && !UNITY_EDITOR
            this.input.onSelect.RemoveListener(OnSelect);
#endif
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        private void OnSelect(string text)
        {
            if(PlatformUtil.IsHuaWeiGroup() || !PlatformUtil.IsMobile())
            {
                return;
            }
            if (UseDialog)
            {
                this.input.text = BridgeHelper.OpenNativeStringDialog((input.placeholder is TMPro.TMP_Text a)?a.text:"", this.input.text);
                DelayInputDeactive().Coroutine();
            }
            else
            {
                BridgeHelper.SetUpOverlayDialog((input.placeholder is TMPro.TMP_Text a)?a.text:"", this.input.text,
                    I18NManager.Instance.I18NGetText(I18NKey.Global_Btn_Confirm),
                    I18NManager.Instance.I18NGetText(I18NKey.Global_Btn_Cancel));
                OverlayHtmlCoroutine().Coroutine();
            }
        }
        private async ETTask DelayInputDeactive()
        {
            await UnityLifeTimeHelper.WaitFrameFinish();
            input.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
        }
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
        }
#endif
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
    }
}