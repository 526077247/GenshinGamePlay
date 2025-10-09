using System;
namespace TaoTie
{
    public class MsgBoxPara
    {
        public string Content;
        public string CancelText;
        public string ConfirmText;
        public Action<UIBaseView> CancelCallback;
        public Action<UIBaseView> ConfirmCallback;
    }
    public class UIMsgBoxWin:UIBaseView,IOnCreate,IOnEnable<MsgBoxPara>,IOnDisable
    {
        public static string PrefabPath => "UI/UIUpdate/Prefabs/UIMsgBoxWin.prefab";
        public UIText Text;
        public UIButton btn_cancel;
        public UIText CancelText;
        public UIButton btn_confirm;
        public UIText ConfirmText;

        private MsgBoxPara para;
        #region overrride

        public void OnCreate()
        {
            this.Text = this.AddComponent<UIText>("Text");
            this.btn_cancel = this.AddComponent<UIButton>("btn_cancel");
            this.CancelText = this.AddComponent<UIText>("btn_cancel/Text");
            this.btn_confirm = this.AddComponent<UIButton>("btn_confirm");
            this.ConfirmText = this.AddComponent<UIText>("btn_confirm/Text");
        }

        public void OnEnable(MsgBoxPara a)
        {
            para = a;
            this.Text.SetText(a.Content);
            this.btn_cancel.SetOnClick(OnClickCancel);
            this.btn_confirm.SetOnClick(OnClickConfirm);
            this.ConfirmText.SetText(a.ConfirmText);
            this.CancelText.SetText(a.CancelText);
        }
        
        public void OnDisable()
        {
            this.btn_cancel.RemoveOnClick();
            this.btn_confirm.RemoveOnClick();
        }
        #endregion
        
        private void OnClickConfirm()
        {
            if (para?.ConfirmCallback != null)
            {
                para.ConfirmCallback.Invoke(this);
            }
        }
        private void OnClickCancel()
        {
            if (para?.CancelCallback != null)
            {
                para.CancelCallback.Invoke(this);
            }
            else
            {
                Close();
            }
        }
        
        void Close()
        {
            UIManager.Instance.CloseBox(this).Coroutine();
        }

    }
}