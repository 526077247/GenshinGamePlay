namespace TaoTie
{
    public class UILoadingView:UIBaseView,IOnCreate
    {
        public static string PrefabPath => "UI/UILoading/Prefabs/UILoadingView.prefab";
        public UISlider slider;

        #region override

        public void OnCreate()
        {
            this.slider = this.AddComponent<UISlider>("Loadingscreen/Slider");
        }

        
       

        #endregion
        public void SetProgress(float value)
        {
            this.slider.SetValue(value);
        }
    }
}