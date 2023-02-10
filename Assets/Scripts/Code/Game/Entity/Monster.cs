namespace TaoTie
{
    /// <summary>
    /// 怪物
    /// </summary>
    public class Monster: Unit,IEntity<int>
    {
        #region IEntity
        
        public override EntityType Type => EntityType.Monster;
        
        public void Init(int configId)
        {
            ConfigId = configId;
            // AddComponent<PlayerInfoComponent, RoleInfo>(info);
            AddCommonUnitComponent();
        }
        
        public void Destroy()
        {
            ConfigId = default;
        }
        
        #endregion
    }
}