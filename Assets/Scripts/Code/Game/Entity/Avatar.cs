namespace TaoTie
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Avatar:Unit,IEntity<int>
    {
        #region override

        public override EntityType Type => EntityType.Avatar;

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