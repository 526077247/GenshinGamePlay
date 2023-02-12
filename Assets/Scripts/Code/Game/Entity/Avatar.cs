namespace TaoTie
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Avatar:Unit,IEntity<int>
    {
        #region IEntity

        public override EntityType Type => EntityType.Avatar;

        public void Init(int configId)
        {
            ConfigId = configId;
            AddCommonUnitComponent();
            AddComponent<PlayerComponent>();
        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion
        
    }
}