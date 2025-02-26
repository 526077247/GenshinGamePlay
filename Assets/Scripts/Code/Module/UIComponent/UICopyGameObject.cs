using System;
using UnityEngine;

namespace TaoTie
{
    public class UICopyGameObject : UIBaseContainer, IOnDestroy
    {
        public CopyGameObject comp;

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

        public void InitListView(int total_count, Action<int, GameObject> ongetitemcallback = null,
            int? start_sibling_index = null)
        {
            this.ActivatingComponent();
            this.comp.InitListView(total_count, ongetitemcallback, start_sibling_index);
        }

        //item是Unity侧的item对象，在这里创建相应的UI对象
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

        //根据Unity侧item获取UI侧的item
        public T GetUIItemView<T>(GameObject item) where T : UIBaseContainer
        {
            return this.GetComponent<T>(item.name);
        }

        public void SetListItemCount(int total_count, int? start_sibling_index = null)
        {
            this.comp.SetListItemCount(total_count, start_sibling_index);
        }

        public void RefreshAllShownItem(int? start_sibling_index = null)
        {
            this.comp.RefreshAllShownItem(start_sibling_index);
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