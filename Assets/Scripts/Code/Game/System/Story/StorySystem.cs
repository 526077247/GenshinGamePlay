using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class StorySystem: IManager<SceneManagerProvider>,IUpdate
    {
        public bool IsPlaying { get; private set; }
        private ConfigStory config;
        public SceneManagerProvider Scene;
        private Vector3 playPos;
        private Quaternion playRot;
        public SceneGroup SceneGroup { get; private set; }

        private List<StoryTimeLineRunner> timeLineStoryRunners;

        #region IManager

        public void Init(SceneManagerProvider scene)
        {
            this.Scene = scene;
            this.timeLineStoryRunners = new List<StoryTimeLineRunner>();
        }

        public void Destroy()
        {
            for (int i = 0; i < timeLineStoryRunners.Count; i++)
            {
                timeLineStoryRunners[i].Dispose();
            }

            timeLineStoryRunners = null;
        }

        public void Update()
        {
            for (int i = 0; i < timeLineStoryRunners.Count; i++)
            {
                timeLineStoryRunners[i]?.Update();
            }
        }

        #endregion

        public async ETTask PlayStory(ulong id,SceneGroup sceneGroup, Vector3 pos, Quaternion rot)
        {
            if(IsPlaying) return;
            IsPlaying = true;
            playPos = pos;
            playRot = rot;
            SceneGroup = sceneGroup;
            config = ConfigStoryCategory.Instance.Get(id);
            if(config == null) return;
            //preload
            if (config.Actors != null)
            {
                using (ListComponent<ETTask> preload = ListComponent<ETTask>.Create())
                {
                    for (int i = 0; i < config.Actors.Length; i++)
                    {
                        preload.Add(config.Actors[i].Preload(this));
                    }
                    await ETTaskHelper.WaitAll(preload);
                }
            }

            await config.Clips.Process(this);
            Messager.Instance.Broadcast(sceneGroup.Id, MessageId.SceneGroupEvent, new StoryPlayOverEvt()
            {
                StoryId = id
            });
            IsPlaying = false;
            SceneGroup = null;
            config = null;
        }

        public async ETTask PlayTimeLine(ConfigStoryTimeLine timeLine)
        {
            if(!IsPlaying) return;
            var runner = StoryTimeLineRunner.Create(this,timeLine,playPos,playRot);
            timeLineStoryRunners.Add(runner);
            await runner.WaitEnd;
        }
        
        public async ETTask<GameObject> Get3dActor(int id)
        {
            for (int i = 0; i < config.Actors.Length; i++)
            {
                if (config.Actors[i].Id == id)
                {
                    return await config.Actors[i].Get3dObj(this);
                }
            }
            return null;
        }
        
        public void Recycle3dActor(int id,GameObject obj)
        {
            for (int i = 0; i < config.Actors.Length; i++)
            {
                if (config.Actors[i].Id == id)
                {
                    config.Actors[i].Recycle3dObj(this,obj);
                }
            }
        }
    }
}