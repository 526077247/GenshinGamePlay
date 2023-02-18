using System;

namespace TaoTie
{
    public class GadgetComponent:Component,IComponent<int,GadgetState>
    {
        public GadgetState GadgetState { get; private set; }
        public int ConfigId { get; private set; }
        public GadgetConfig Config => GadgetConfigCategory.Instance.Get(ConfigId);
        public event Action<GadgetState, GadgetState> onGadgetStateChange;
        #region IComponent
        public void Init(int p1,GadgetState state)
        {
            ConfigId = p1;
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

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="state"></param>
        public void SetGadgetState(GadgetState state)
        {
            var oldState = GadgetState;
            GadgetState = state;
            onGadgetStateChange?.Invoke(oldState,state);
        }
    }
}