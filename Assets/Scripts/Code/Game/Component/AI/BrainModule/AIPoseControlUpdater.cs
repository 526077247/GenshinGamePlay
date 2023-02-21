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
            var poseFsm = knowledge.aiOwnerEntity?.GetComponent<PoseFSMComponent>();
            if (poseFsm != null)
            {
                if (knowledge.poseID != poseFsm.defaultFsm.PoseID)
                {
                    var old = knowledge.poseID;
                    knowledge.poseID = poseFsm.defaultFsm.PoseID;
                    Messager.Instance.Broadcast(knowledge.aiOwnerEntity.Id,MessageId.PoseChange,old,knowledge.poseID);
                    knowledge.facingMoveTactic.SwitchSetting(knowledge.poseID);
                }
            }
        }
    }
}