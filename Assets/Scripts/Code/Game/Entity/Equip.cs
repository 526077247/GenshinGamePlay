namespace TaoTie
{
    public class Equip : Unit,IEntity<int>
    {

        #region IEntity

        public override EntityType Type => EntityType.Equip;

        public void Init(int configId)
        {
            var weapon = AddComponent<EquipComponent,int>(configId);
            ConfigId = weapon.Config.UnitId;
            AddComponent<AttachComponent>();
            AddComponent<UnitModelComponent,ConfigModel>(null);
            AddComponent<FsmComponent,ConfigFsmController>(GetFsmConfig(Config.FSM));
        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion
    }
}