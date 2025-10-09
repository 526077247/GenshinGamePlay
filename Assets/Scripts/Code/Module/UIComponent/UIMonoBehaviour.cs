namespace TaoTie
{
    public class UIMonoBehaviour<T> : UIBaseContainer where T : UnityEngine.Behaviour
    {
        private T component;
        void ActivatingComponent()
        {
            if (this.component == null)
            {
                this.component = this.GetGameObject().GetComponent<T>();
                if (this.component == null)
                {
                    Log.Error($"添加UI侧组件UIMonoBehaviour时，物体{this.GetGameObject().name}上没有找到{TypeInfo<T>.TypeName}组件");
                }
              
            }
        }

        public void SetEnable(bool enable)
        {
            ActivatingComponent();
            component.enabled = enable;
        }
        
        public T GetComponent()
        {
            ActivatingComponent();
            return component;
        }
    }
}