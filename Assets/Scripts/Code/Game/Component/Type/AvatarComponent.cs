namespace TaoTie
{
    public class AvatarComponent: Component,IComponent<int>
    {
        public int ConfigId { get; private set; }
        public AvatarConfig Config => AvatarConfigCategory.Instance.Get(ConfigId);
        #region IComponent

        public void Init(int p1)
        {
            ConfigId = p1;
        }

        public void Destroy()
        {
            ConfigId = 0;
        }

        #endregion
    }
}