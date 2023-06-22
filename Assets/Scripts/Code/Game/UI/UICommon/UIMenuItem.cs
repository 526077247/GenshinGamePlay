using System;
using UnityEngine;

namespace TaoTie
{
    public class UIMenuItem: UIBaseContainer,IOnCreate
    {
        public MenuPara Para;
        public int Index;
        public Action<int, int> onClick;

        public UIText Text;
        public UIImage TabFocus;
        public UIPointerClick Btn;

        #region override

        public void OnCreate()
        {
            Text = AddComponent<UIText>("Content/Text");
            TabFocus = AddComponent<UIImage>("TabFocus");
            Btn = AddComponent<UIPointerClick>();
            Btn.SetOnClick(() =>
            {
                onClick?.Invoke(Para.Id, Index);
            });
        }

        #endregion
        
        public void SetData(MenuPara para, int index, Action<int, int> onClick, bool isActive = false)
        {
            this.onClick = onClick;
            Index = index;
            Para = para;
            Text.SetActive(!string.IsNullOrEmpty(para.Name));
            Text.SetText(para.Name);
            SetIsActive(isActive);
        }
        /// <summary>
        /// 设置是否选择状态
        /// </summary>
        /// <param name="isActive"></param>
        public void SetIsActive(bool isActive = true)
        {
            TabFocus.SetActive(isActive);
            Text.SetTextColor(isActive ? Color.white : Color.black);
        }
    }
}