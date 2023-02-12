namespace TaoTie
{
    /// <summary>
    /// 做一些技能生成物、小动物、场景可交互物件什么的小工具
    /// </summary>
    public class Gadget: Unit,IEntity<int>,IEntity<int,GadgetState>
    {
        public override EntityType Type => EntityType.Gadget;

        #region IEntity

        public void Init(int id)
        {
            ConfigId = id;
            AddComponent<GadgetComponent>();
        }
        public void Init(int id,GadgetState state)
        {
            ConfigId = id;
            AddComponent<GadgetComponent,GadgetState>(state);
        }
        public void Destroy()
        {
            
        }

        #endregion
    }
}