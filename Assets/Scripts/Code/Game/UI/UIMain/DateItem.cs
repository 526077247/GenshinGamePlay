using System;
using UnityEngine.UI;

namespace TaoTie
{
    public class DateItem: UIBaseContainer,IOnCreate
    {
        public UIText Text;

        #region override

        public void OnCreate()
        {
            Text = AddComponent<UIText>("Text");
        }

        #endregion


        public void SetData(int index)
        {
            Text.SetText(DateTime.Now.AddDays(index).ToString("yyyy-M-d dddd"));
        }
    }
}