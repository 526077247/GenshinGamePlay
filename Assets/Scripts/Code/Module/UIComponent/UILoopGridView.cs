using SuperScrollView;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace TaoTie
{
    public class UILoopGridView : UIBaseContainer, IOnDestroy
    {
        private LoopGridView loopGridView;

        #region override

        public void OnDestroy()
        {
            if (this.loopGridView != null)
            {
                this.loopGridView.ClearListView();
                this.loopGridView = null;
            }
        }

        #endregion

        public void ActivatingComponent()
        {
            if (this.loopGridView == null)
            {
                this.loopGridView = this.GetGameObject().GetComponent<LoopGridView>();
                if (this.loopGridView == null)
                {
                    Log.Error($"添加UI侧组件UILoopGridView时，物体{this.GetGameObject().name}上没有找到LoopGridView组件");
                }
            }
        }

        public void InitGridView(int itemTotalCount,
            System.Func<LoopGridView, int, int, int, LoopGridViewItem> onGetItemByRowColumn,
            LoopGridViewSettingParam settingParam = null,
            LoopGridViewInitParam initParam = null)
        {
            this.ActivatingComponent();
            this.loopGridView.InitGridView(itemTotalCount, onGetItemByRowColumn, settingParam, initParam);
        }

        //item是Unity侧的item对象，在这里创建相应的UI对象
        public T AddItemViewComponent<T>(LoopGridViewItem item) where T : UIBaseContainer
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
        public T GetUIItemView<T>(LoopGridViewItem item) where T : UIBaseContainer
        {
            return this.GetComponent<T>(item.gameObject.name);
        }

        //itemCount重设item的数量，resetPos是否刷新当前显示的位置
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            this.ActivatingComponent();
            this.loopGridView.SetListItemCount(itemCount, resetPos);
        }

        //获取当前index对应的item 没有显示的话返回null
        public LoopGridViewItem GetShownItemByItemIndex(int itemIndex)
        {
            this.ActivatingComponent();
            return this.loopGridView.GetShownItemByItemIndex(itemIndex);
        }

        public void MovePanelToItemByRowColumn(int row, int column, int offsetX = 0, int offsetY = 0)
        {
            this.ActivatingComponent();
            this.loopGridView.MovePanelToItemByRowColumn(row, column, offsetX, offsetY);
        }


        public void RefreshAllShownItem()
        {
            this.ActivatingComponent();
            this.loopGridView.RefreshAllShownItem();
        }

        public void SetItemSize(Vector2 sizeDelta)
        {
            this.ActivatingComponent();
            this.loopGridView.SetItemSize(sizeDelta);
        }

        public void SetOnBeginDragAction(Action<PointerEventData> callback)
        {
            this.ActivatingComponent();
            this.loopGridView.mOnBeginDragAction = callback;
        }

        public void SetOnDragingAction(Action<PointerEventData> callback)
        {
            this.ActivatingComponent();
            this.loopGridView.mOnDragingAction = callback;
        }

        public void SetOnEndDragAction(Action<PointerEventData> callback)
        {
            this.ActivatingComponent();
            this.loopGridView.mOnEndDragAction = callback;
        }
        
        public void RemoveUIItemAllComponent(GameObject obj)
        {
            RemoveAllComponent(obj.name);
        }
                        
        public void CleanUp(string name)
        {
            if(loopGridView == null) return;
            loopGridView.CleanUp(name, RemoveUIItemAllComponent);
        }
    }
}