namespace TaoTie
{
    public class MonsterComponent: Component,IComponent<int>
    {
        public int ConfigId { get; private set; }
        public MonsterConfig Config => MonsterConfigCategory.Instance.Get(ConfigId);
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