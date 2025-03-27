using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TaoTie
{
    public class UIEventTrigger:UIBaseContainer,IOnDestroy
    {
        private EventTrigger EventTrigger;
        private Dictionary<EventTriggerType, UnityAction<BaseEventData>> events = new ();

        #region override
        public void OnDestroy()
        {
            if (EventTrigger != null)
            {
                for (int i = 0; i < this.EventTrigger.triggers.Count; i++)
                {
                    var trigger = this.EventTrigger.triggers[i];
                    if (events.TryGetValue(trigger.eventID, out var evt))
                    {
                        trigger.callback.RemoveListener(evt);
                        events.Remove(trigger.eventID);
                    }
                }
            }
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.EventTrigger == null)
            {
                this.EventTrigger = this.GetGameObject().GetComponent<EventTrigger>();
                if (this.EventTrigger == null)
                {
                    this.EventTrigger = this.GetGameObject().AddComponent<EventTrigger>();
                    Log.Info($"添加UI侧组件UIEventTrigger时，物体{this.GetGameObject().name}上没有找到EventTrigger组件");
                }
            }
        }

        public void SetEnabled(bool flag)
        {
            this.ActivatingComponent();
            this.EventTrigger.enabled = flag;
        }
        public void AddEvent(EventTriggerType triggerType, UnityAction<PointerEventData> callback)
        {
            this.ActivatingComponent();
            RemoveEvent(triggerType);
            EventTrigger.Entry onPointerEnterEntry = null;
            for (int i = 0; i < this.EventTrigger.triggers.Count; i++)
            {
                if (this.EventTrigger.triggers[i].eventID == triggerType)
                {
                    onPointerEnterEntry = this.EventTrigger.triggers[i];
                    break;
                }
            }

            if (onPointerEnterEntry == null)
            {
                onPointerEnterEntry = new EventTrigger.Entry()
                {
                    eventID = triggerType,
                    callback = new EventTrigger.TriggerEvent()
                };
                this.EventTrigger.triggers.Add(onPointerEnterEntry);
            }
            events[triggerType] = (a) =>
            {
                callback?.Invoke(a as PointerEventData);
            };
            onPointerEnterEntry.callback.AddListener(events[triggerType]);
        }
        
        public void RemoveEvent(EventTriggerType triggerType)
        {
            if (events.TryGetValue(triggerType, out var evt))
            {
                for (int i = 0; i < this.EventTrigger.triggers.Count; i++)
                {
                    if (this.EventTrigger.triggers[i].eventID == triggerType)
                    {
                        this.EventTrigger.triggers[i].callback.RemoveListener(evt);
                        events.Remove(triggerType);
                        break;
                    }
                }
            }
        }

        public void AddOnPointerEnter(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.PointerEnter, callback);
        }
        
        public void AddOnPointerExit(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.PointerExit, callback);
        }
        
        public void AddOnDrag(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.Drag, callback);
        }
        
        public void AddOnDrop(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.Drop, callback);
        }
        
        public void AddOnPointerDown(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.PointerDown, callback);
        }
        
        public void AddOnPointerUp(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.PointerUp, callback);
        }
        
        public void AddOnPointerClick(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.PointerClick, callback);
        }

        public void AddOnScroll(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.Scroll, callback);
        }
        
        public void AddOnInitializePotentialDrag(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.InitializePotentialDrag, callback);
        }
        
        public void AddOnBeginDrag(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.BeginDrag, callback);
        }
        
        public void AddOnEndDrag(UnityAction<PointerEventData> callback)
        {
            AddEvent(EventTriggerType.EndDrag, callback);
        }
        
        public void RemoveOnPointerEnter()
        {
            RemoveEvent(EventTriggerType.PointerEnter);
        }
        
        public void RemoveOnPointerExit()
        {
            RemoveEvent(EventTriggerType.PointerExit);
        }
        
        public void RemoveOnDrag()
        {
            RemoveEvent(EventTriggerType.Drag);
        }
        
        public void RemoveOnDrop()
        {
            RemoveEvent(EventTriggerType.Drop);
        }
        
        public void RemoveOnPointerDown()
        {
            RemoveEvent(EventTriggerType.PointerDown);
        }
        
        public void RemoveOnPointerUp()
        {
            RemoveEvent(EventTriggerType.PointerUp);
        }
        
        public void RemoveOnPointerClick()
        {
            RemoveEvent(EventTriggerType.PointerClick);
        }
        
        public void RemoveOnScroll()
        {
            RemoveEvent(EventTriggerType.Scroll);
        }
        
        public void RemoveOnInitializePotentialDrag()
        {
            RemoveEvent(EventTriggerType.InitializePotentialDrag);
        }
        
        public void RemoveOnBeginDrag()
        {
            RemoveEvent(EventTriggerType.BeginDrag);
        }
        
        public void RemoveOnEndDrag()
        {
            RemoveEvent(EventTriggerType.EndDrag);
        }
    }
}