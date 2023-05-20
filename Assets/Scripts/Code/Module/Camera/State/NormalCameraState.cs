using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class NormalCameraState: CameraState
    {
        public override bool IsBlenderState => false;

        public ConfigCamera Config { get; private set; }
        
        private CameraPluginRunner body;
        private CameraPluginRunner head;
        private ListComponent<CameraPluginRunner> others;

        public Transform follow { get; private set; }
        public Transform target { get; private set; }

        public static NormalCameraState Create(ConfigCamera config, int priority)
        {
            NormalCameraState res = ObjectPool.Instance.Fetch<NormalCameraState>();
            res.Priority = priority;
            res.Id = IdGenerater.Instance.GenerateId();
            res.Config = config;
            res.Data = CameraStateData.Create();
            res.Data.Fov = res.Config.Fov;
            res.Data.NearClipPlane = res.Config.NearClipPlane;
            res.IsOver = false;
            res.CreateRunner();
            return res;
        }

        private void CreateRunner()
        {
            if (Config.HeadPlugin != null)
            {
                head = CameraManager.Instance.CreatePluginRunner(Config.HeadPlugin, this);
            }

            if (Config.BodyPlugin != null)
            {
                body =  CameraManager.Instance.CreatePluginRunner(Config.BodyPlugin, this);
            }
            
            if (Config.OtherPlugin != null)
            {
                others = ListComponent<CameraPluginRunner>.Create();
                for (int i = 0; i < Config.OtherPlugin.Length; i++)
                {
                    if(Config.OtherPlugin[i] == null) continue;
                    others.Add(CameraManager.Instance.CreatePluginRunner(Config.OtherPlugin[i], this));
                }
            }
        }
        
        public override void Update()
        {
            if(IsOver) return;
            head?.Update();
            body?.Update();
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    others[i]?.Update();
                }
            }
        }

        public override void Dispose()
        {
            CameraManager.Instance.RemoveState(Id);
            //base
            Id = default;
            Priority = default;
            Data.Dispose();
            Data = null;
            IsOver = true;
            
            //this
            target = null;
            follow = null;
            body.Dispose();
            body = null;
            head.Dispose();
            head = null;
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    others[i].Dispose();
                }
                others.Dispose();
                others = null;
            }
            
            ObjectPool.Instance.Recycle(this);
        }

        public void SetTarget(Transform transform)
        {
            this.target = transform;
        }

        public void SetFollow(Transform transform)
        {
            this.follow = transform;
        }
    }
}