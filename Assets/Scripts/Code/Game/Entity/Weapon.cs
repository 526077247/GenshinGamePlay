namespace TaoTie
{
    public class Weapon : Entity
    {
        public int ConfigId { get; protected set; } //配置表id

        public WeaponConfig Config => WeaponConfigCategory.Instance.Get(this.ConfigId);
        #region IEntity

        public override EntityType Type => EntityType.Weapon;

        public void Init()
        {
            
        }

        public void Destroy()
        {
            
        }

        #endregion
    }
}