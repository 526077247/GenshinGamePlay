namespace TaoTie
{
    public class EquipComponent: Component,IComponent<int>
    {
        public int ConfigId { get; protected set; } //配置表id

        public EquipConfig Config => EquipConfigCategory.Instance.Get(this.ConfigId);
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