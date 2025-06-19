using SuperScrollView;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TaoTie
{
    public class UILoopListView2 : UIBaseContainer, IOnDestroy
    {
        private LoopListView2 loopListView;

        #region override

        public void OnDestroy()
        {
            if (this.loopListView != null)
            {
                loopListView.ClearListView();
                loopListView = null;
            }
        }

        #endregion

        public void ActivatingComponent()
        {
            if (this.loopListView == null)
            {
                this.loopListView = this.GetGameObject().GetComponent<LoopListView2>();
                if (this.loopListView == null)
                {
                    Log.Error($"添加UI侧组件UILoopListView2时，物体{this.GetGameObject().name}上没有找到LoopListView2组件");
                }
            }
        }

        public void InitListView(int itemTotalCount,
            System.Func<LoopListView2, int, LoopListViewItem2> onGetItemByIndex,
            LoopListViewInitParam initParam = null)
        {
            this.ActivatingComponent();
            this.loopListView.InitListView(itemTotalCount, onGetItemByIndex, initParam);
        }


        //item是Unity侧的item对象，在这里创建相应的UI对象
        public T AddItemViewComponent<T>(LoopListViewItem2 item) where T : UIBaseContainer
        {
            //保证名字不能相同 不然没法cache
            item.gameObject.name = item.gameObject.name + item.ItemId;
            T t = this.AddComponentNotCreate<T>(item.gameObject.name);
            t.SetTransform(item.transform);
            if (t is IOnCreate a)
                a.OnCreate();
            if (ActiveSelf)
                t.SetActive(true);
            if (t is II18N i18n)
                I18NManager.Instance.RegisterI18NEntity(i18n);
            return t;
        }

        //根据Unity侧item获取UI侧的item
        public T GetUIItemView<T>(LoopListViewItem2 item) where T : UIBaseContainer
        {
            return this.GetComponent<T>(item.gameObject.name);
        }

        //itemCount重设item的数量，resetPos是否刷新当前显示的位置
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            this.ActivatingComponent();
            this.loopListView.SetListItemCount(itemCount, resetPos);
        }

        //获取当前index对应的item 没有显示的话返回null
        public LoopListViewItem2 GetShownItemByItemIndex(int itemIndex)
        {
            this.ActivatingComponent();
            return this.loopListView.GetShownItemByItemIndex(itemIndex);
        }

        public void RefreshAllShownItem()
        {
            this.ActivatingComponent();
            this.loopListView.RefreshAllShownItem();
        }

        public void SetOnBeginDragAction(Action<PointerEventData> callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnBeginDragAction = callback;
        }

        public void SetOnDragingAction(Action<PointerEventData> callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnDragingAction = callback;
        }

        public void SetOnEndDragAction(Action<PointerEventData> callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnEndDragAction = callback;
        }

        public void MovePanelToItemIndex(int index, float offset = 0)
        {
            this.ActivatingComponent();
            this.loopListView.MovePanelToItemIndex(index, offset);
        }
        
        public void SetSnapTargetItemIndex(int index, float moveMaxAbsVec = -1)
        {
            ActivatingComponent();
            loopListView.SetSnapTargetItemIndex(index, moveMaxAbsVec);
        }
        public int GetSnapTargetItemIndex()
        {
            ActivatingComponent();
            return loopListView.CurSnapNearestItemIndex;
        }
        public void SetSnapMaxAbsVec(float maxAbsVec)
        {
            ActivatingComponent();
            loopListView.SnapMoveDefaultMaxAbsVec = maxAbsVec;
        }


        public void SetOnSnapChange(Action<LoopListView2, LoopListViewItem2> callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnSnapNearestChanged = callback;
        }
        
        public LoopListView2 GetLoopListView()
        {
            ActivatingComponent();
            return this.loopListView;
        }

        public ScrollRect GetScrollRect()
        {
            ActivatingComponent();
            return loopListView.ScrollRect;
        }

        public void RemoveUIItemAllComponent(GameObject obj)
        {
            RemoveAllComponent(obj.name);
        }
                        
        public void CleanUp(string name)
        {
            if(loopListView == null) return;
            loopListView.CleanUp(name, RemoveUIItemAllComponent);
        }
    }
}