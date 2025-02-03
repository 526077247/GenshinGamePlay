using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using YooAsset;

namespace TaoTie
{
    public class StoryTimeLineRunner: IDisposable
    {
        private StorySystem storySystem;
        private GameObject playObj;
        private PlayableDirector playableDirector;
        private ConfigStoryTimeLine config;
        private TimelineAsset timelineAsset;
        public ETTask WaitEnd { get; private set; }
        public long StartTime{ get; private set; }
        private Vector3 playPos;
        private Quaternion playRot;
        private int clipIndex;
        private readonly Dictionary<string, PlayableBinding> bindings = new Dictionary<string, PlayableBinding>();
        private readonly Dictionary<int, GameObject> actor3d = new Dictionary<int, GameObject>();
        public bool IsDispose { get; private set; }
        public static StoryTimeLineRunner Create(StorySystem storySystem, ConfigStoryTimeLine timeLine,Vector3 pos, Quaternion rot)
        {
            var res = ObjectPool.Instance.Fetch<StoryTimeLineRunner>();
            res.config = timeLine;
            res.WaitEnd = ETTask.Create(true);
            res.StartTime = GameTimerManager.Instance.GetTimeNow();
            res.playPos = pos;
            res.playRot = rot;
            res.storySystem = storySystem;
            res.clipIndex = 0;
            res.Init().Coroutine();
            return res;
        }

        private async ETTask Init()
        {
            IsDispose = false;
            timelineAsset = await ResourcesManager.Instance.LoadAsync<TimelineAsset>(config.Path);
            if (IsDispose)
            {
                ResourcesManager.Instance.ReleaseAsset(timelineAsset);
                timelineAsset = null;
                return;
            }
            foreach (var o in timelineAsset.outputs)
            {
                bindings.Add(o.streamName,o);
            }
            playObj = new GameObject(timelineAsset.name);
            playObj.transform.position = playPos;
            playObj.transform.rotation = playRot;
            playableDirector = playObj.AddComponent<PlayableDirector>();
            playableDirector.playableAsset = timelineAsset;
            playableDirector.Play();
            foreach (var item in config.Binding)
            {
                SetBinding(item.Key, item.Value).Coroutine();
            }
        }
        public void Dispose()
        {
            if(IsDispose)return;
            IsDispose = true;
            WaitEnd?.SetResult();
            WaitEnd = null;
            config = null;
            playPos = default;
            playRot = default;
            StartTime = 0;
            clipIndex = 0;
            playableDirector?.Stop();
            foreach (var item in actor3d)
            {
                storySystem.Recycle3dActor(item.Key, item.Value);
            }
            actor3d.Clear();
            bindings.Clear();
            storySystem = null;
            
            if (timelineAsset != null)
            {
                ResourcesManager.Instance.ReleaseAsset(timelineAsset);
                timelineAsset = null;
                GameObject.Destroy(playObj);
                playableDirector = null;
                playObj = null;
                GameObjectPoolManager.GetInstance().Cleanup();
                YooAssetsMgr.Instance.UnloadUnusedAssets();
            }
        }

        public void Update()
        {
            if (IsDispose || timelineAsset == null) return;
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            var offset = (timeNow - StartTime)/1000f;
            
            playableDirector.time = offset;
            if (config.Clips != null)
            {
                for (; clipIndex < config.Clips.Length; clipIndex++)
                {
                    if (config.Clips[clipIndex].StartTime <= offset)
                    {
                        config.Clips[clipIndex].Process(offset, this);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (offset > timelineAsset.duration)
            {
                playableDirector.time = playableDirector.duration;
                Dispose();
            }
        }
        
        public async ETTask SetBinding(string trackName, int actorId)
        {
            var obj = await Get3dActor(actorId);
            if (obj != null)
            {
                obj.transform.parent = playObj.transform;
                playableDirector.SetGenericBinding(bindings[trackName].sourceObject,
                    obj.GetComponent(bindings[trackName].outputTargetType));
            }
        }
        
        public async ETTask<GameObject> Get3dActor(int id)
        {
            if (actor3d.TryGetValue(id, out var res))
            {
                return res;
            }
            res = await storySystem.Get3dActor(id);
            if (IsDispose)
            {
                storySystem.Recycle3dActor(id, res);
                return null;
            }
            
            actor3d.Add(id,res);
            return res;
        }
    }
}