using SuperScrollView;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (activeSelf && t is IOnEnable b)
                b.OnEnable();
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

        public void SetOnBeginDragAction(Action callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnBeginDragAction = callback;
        }

        public void SetOnDragingAction(Action callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnDragingAction = callback;
        }

        public void SetOnEndDragAction(Action callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnEndDragAction = callback;
        }

        public void MovePanelToItemIndex(int index, float offset = 0)
        {
            this.ActivatingComponent();
            this.loopListView.MovePanelToItemIndex(index, offset);
        }

        public void SetOnSnapChange(Action<LoopListView2, LoopListViewItem2> callback)
        {
            this.ActivatingComponent();
            this.loopListView.mOnSnapNearestChanged = callback;
        }
    }
}