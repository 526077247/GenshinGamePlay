using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class InteeComponent: Component, IComponent<ConfigIntee>, IUpdate
    {
        public event Action<int> OnTouch;
        
        private ConfigIntee config;

        private Avatar avatar;
        private bool enable;
        private long nextCheckTime = 0;
        private Dictionary<int, bool> enableState;
        private Dictionary<int, ConfigInteeItem> showItems;
        #region IComponent

        public void Init(ConfigIntee conf)
        {
            enable = conf.DefaultEnable;
            config = conf;
            if (SceneManager.Instance.CurrentScene is MapScene scene)
            {
                avatar = scene.Self as Avatar;
            }

            showItems = new Dictionary<int, ConfigInteeItem>();
            enableState = new Dictionary<int, bool>();
            if (config.Params != null)
            {
                for (int i = 0; i < config.Params.Length; i++)
                {
                    enableState.Add(config.Params[i].LocalId,config.Params[i].DefaultEnable);
                }
            }
        }

        public void Destroy()
        {
            foreach (var item in showItems)
            {
                Messager.Instance.Broadcast(0,MessageId.ShowIntee,Id,false,item.Value);
            }
            config = null;
            enableState = null;
            showItems = null;
            nextCheckTime = 0;
        }

        #endregion

        public void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            var timeNow = TimerManager.Instance.GetTimeNow();
            if(timeNow < nextCheckTime) return;
            if (avatar != null)
            {
                if (enable && config.Params != null && config.Params.Length > 0)
                {
                    var posSelf = GetParent<Unit>().Position + config.Offset;
                    var otherPos = avatar.Position;
                    var different = posSelf - otherPos;
                    if (different.y > config.Height || different.y < -config.Height)
                    {
                        ShowIntee(false);
                        return;
                    }

                    different.y = 0;
                    var dis = Vector3.Magnitude(different);
                    
                    if (dis > config.Radius)
                    {
                        var interval = (int) ((dis - config.Radius) * 100);
                        if (interval > 2000) interval = 2000;
                        nextCheckTime = timeNow + interval;
                        ShowIntee(false);
                        return;
                    }
                    nextCheckTime = timeNow;
                    ShowIntee(true);
                }
                else
                {
                    ShowIntee(false);
                }
            }   
        }
        
        private void ShowIntee(bool show)
        {
            if(IsDispose) return;
            if (config.Params != null)
            {
                for (int i = 0; i < config.Params.Length; i++)
                {
                    var localId = config.Params[i].LocalId;
                    if (showItems.ContainsKey(localId))
                    {
                        if (!show || !enableState[localId])
                        {
                            showItems.Remove(localId);
                            Messager.Instance.Broadcast(0,MessageId.ShowIntee,Id,false,config.Params[i]);
                        }
                    }
                    else
                    {
                        if (show && enableState[localId])
                        {
                            showItems.Add(localId,config.Params[i]);
                            Messager.Instance.Broadcast(0,MessageId.ShowIntee,Id,true,config.Params[i]);
                        }
                    }
                }
            }

        }
        
        
        public void SetEnable(bool enable)
        {
            this.enable = enable;
            Refresh();
        }
        
        public void SetItemEnable(bool enable, int localId)
        {
            this.enableState[localId] = enable;
            Refresh();
        }

        public void OnClickIntee(int localId)
        {
            OnTouch?.Invoke(localId);
        }
    }
}