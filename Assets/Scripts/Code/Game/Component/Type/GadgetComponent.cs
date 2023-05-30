using System;

namespace TaoTie
{
    public class GadgetComponent:Component,IComponent<int,GadgetState>
    {
        public GadgetState GadgetState { get; private set; }
        public int ConfigId { get; private set; }
        public GadgetConfig Config => GadgetConfigCategory.Instance.Get(ConfigId);
        public event Action<GadgetState, GadgetState> onGadgetStateChange;
        private FsmComponent fsmComponent => parent.GetComponent<FsmComponent>();
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
            fsmComponent.SetData(FSMConst.GadgetState,(int)state);
            onGadgetStateChange?.Invoke(oldState,state);
            var sgac = parent.GetComponent<SceneGroupActorComponent>();
            if (sgac != null)
            {
                Messager.Instance.Broadcast(sgac.SceneGroup.Id,MessageId.SceneGroupEvent,new GadgetStateChangeEvt()
                {
                    GadgetId = sgac.LocalId,
                    OldState = oldState,
                    State = state
                });
            }
        }
    }
}