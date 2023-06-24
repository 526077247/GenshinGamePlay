namespace TaoTie
{
    public class StorySystem: IManager<SceneManagerProvider>
    {
        private SceneManagerProvider scene;
        #region IManager

        public void Init(SceneManagerProvider scene)
        {
            this.scene = scene;
        }

        public void Destroy()
        {
            
        }

        #endregion

        public async ETTask PlayStory(ulong id,long sceneGroupId)
        {
            var config = ConfigStoryCategory.Instance.Get(id);
            if(config==null) return;
            await config.Clips.Process();
            Messager.Instance.Broadcast(sceneGroupId,MessageId.SceneGroupEvent,new StoryPlayOverEvt()
            {
                StoryId = id
            });
        }
    }
}