using System;

namespace TaoTie
{
    public class CellItem: UIBaseContainer,IOnCreate
    {
        private UIText Text;
        #region override

        public void OnCreate()
        {
            Text = AddComponent<UIText>("Text");
        }

        #endregion

        public void SetData(DateTime time)
        {
            Text.SetText(time.Day.ToString());
        }
    }
}