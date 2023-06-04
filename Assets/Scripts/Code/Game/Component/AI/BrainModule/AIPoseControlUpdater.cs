namespace TaoTie
{
    /// <summary>
    /// Pose状态机
    /// </summary>
    public class AIPoseControlUpdater: BrainModuleBase
    {
        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            var poseFsm = knowledge.Entity?.GetComponent<PoseFSMComponent>();
            if (poseFsm != null)
            {
                if (knowledge.PoseID != poseFsm.DefaultFsm.PoseID)
                {
                    var old = knowledge.PoseID;
                    knowledge.PoseID = poseFsm.DefaultFsm.PoseID;
                    knowledge.FacingMoveTactic.SwitchSetting(knowledge.PoseID);
                    knowledge.MeleeChargeTactic.SwitchSetting(knowledge.PoseID);
                    knowledge.FleeTactic.SwitchSetting(knowledge.PoseID);
                    knowledge.WanderTactic.SwitchSetting(knowledge.PoseID);
                    Messager.Instance.Broadcast(knowledge.Entity.Id,MessageId.PoseChange,old,knowledge.PoseID);
                }
            }
        }
    }
}