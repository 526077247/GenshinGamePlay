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
            var poseFsm = knowledge.AiOwnerEntity?.GetComponent<PoseFSMComponent>();
            if (poseFsm != null)
            {
                if (knowledge.PoseID != poseFsm.DefaultFsm.PoseID)
                {
                    var old = knowledge.PoseID;
                    knowledge.PoseID = poseFsm.DefaultFsm.PoseID;
                    Messager.Instance.Broadcast(knowledge.AiOwnerEntity.Id,MessageId.PoseChange,old,knowledge.PoseID);
                    knowledge.FacingMoveTactic.SwitchSetting(knowledge.PoseID);
                }
            }
        }
    }
}