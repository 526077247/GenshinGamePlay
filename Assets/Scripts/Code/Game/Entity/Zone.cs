namespace TaoTie
{
    public class Zone: Unit,IEntity
    {
        #region IEntity

        public override EntityType Type => EntityType.Zone;

        public void Init()
        {
            ConfigId = -1;
            AddComponent<GameObjectHolderComponent>();
        }

        public void Destroy()
        {
            ConfigId = 0;
        }

        #endregion
    }
}