using System;
using UnityEngine;

namespace TaoTie
{
    public class UICopyGameObject : UIBaseContainer, IOnDestroy
    {
        private CopyGameObject comp;

        #region override

        public void OnDestroy()
        {
            this.comp?.Clear();
        }

        #endregion

        void ActivatingComponent()
        {
            if (this.comp == null)
            {
                this.comp = this.GetGameObject().GetComponent<CopyGameObject>();
                if (this.comp == null)
                {
                    this.comp = this.GetGameObject().AddComponent<CopyGameObject>();
                    Log.Error($"添加UI侧组件UICopyGameObject时，物体{this.GetGameObject().name}上没有找到CopyGameObject组件");
                }
            }
        }

        public void InitListView(int totalCount, Action<int, GameObject> onGetItemCallback = null,
            int? startSiblingIndex = null)
        {
            this.ActivatingComponent();
            this.comp.InitListView(totalCount, onGetItemCallback, startSiblingIndex);
        }

        /// <summary>
        /// item是Unity侧的item对象，在这里创建相应的UI对象
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddItemViewComponent<T>(GameObject item) where T : UIBaseContainer
        {
            //保证名字不能相同 不然没法cache
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

        /// <summary>
        /// 根据Unity侧item获取UI侧的item
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUIItemView<T>(GameObject item) where T : UIBaseContainer
        {
            return this.GetComponent<T>(item.name);
        }

        public void SetListItemCount(int totalCount, int? startSiblingIndex = null)
        {
            this.comp.SetListItemCount(totalCount, startSiblingIndex);
        }

        public void RefreshAllShownItem(int? startSiblingIndex = null)
        {
            this.comp.RefreshAllShownItem(startSiblingIndex);
        }

        public GameObject GetItemByIndex(int index)
        {
            return this.comp.GetItemByIndex(index);
        }

        public int GetListItemCount()
        {
            return this.comp.GetListItemCount();
        }
    }
}