namespace TaoTie
{
    public class GadgetComponent:Component,IComponent,IComponent<GadgetState>
    {
        public GadgetState GadgetState { get; private set; }
        #region IComponent

        public void Init(GadgetState state)
        {
            GadgetState = state;
        }
        public void Init()
        {
            GadgetState = GadgetState.Default;
        }
        public void Destroy()
        {
            
        }

        #endregion
    }
}