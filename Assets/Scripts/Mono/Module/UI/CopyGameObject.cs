using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class CopyGameObject : MonoBehaviour
    {
        int? startSiblingIndex;
        Action<int, GameObject> onGetItemCallback;
        int showCount;
        List<GameObject> itemViewList = new List<GameObject>();
        public GameObject item;//要复制的物体
        private void Awake()
        {
            if (item.transform.parent == transform)
            {
                this.startSiblingIndex = item.transform.GetSiblingIndex();
            }
        }

        private void OnEnable()
        {
            item.SetActive(false);
        }

        public void InitListView(int totalCount, Action<int, GameObject> onGetItemCallback = null, int? startSiblingIndex = null)
        {
            //for (int i = 0; i < m_itemviewlist.Count; i++)
            //    Destroy(m_itemviewlist[i]);
            //m_itemviewlist.Clear();
            if (startSiblingIndex != null)
            {
                this.startSiblingIndex = startSiblingIndex;
                if (this.startSiblingIndex > transform.childCount)
                {
                    this.startSiblingIndex = this.transform.childCount - 1;
                }
            }
            this.onGetItemCallback = onGetItemCallback;
            SetListItemCount(totalCount);
        }

        public void SetListItemCount(int totalCount, int? startSiblingIndex = null)
        {
            if (totalCount > 10) Debug.Log("total_count 不建议超过10个");
            if (item == null) Debug.LogError("item is Null!!!");
            if (startSiblingIndex != null)
            {
                this.startSiblingIndex = startSiblingIndex;
                if (this.startSiblingIndex > transform.childCount)
                {
                    this.startSiblingIndex = this.transform.childCount - 1;
                }
            }
            this.showCount = totalCount;
            var count = this.itemViewList.Count > totalCount ? this.itemViewList.Count : totalCount;
            for (int i = 0; i < count; i++)
            {
                if (i < this.itemViewList.Count && i < totalCount)
                {
                    this.itemViewList[i].SetActive(true);
                    if (this.startSiblingIndex != null)
                    {
                        this.itemViewList[i].transform.SetSiblingIndex((int)this.startSiblingIndex + i);
                    }
                    this.onGetItemCallback?.Invoke(i, this.itemViewList[i]);
                }
                else if (i < totalCount)
                {
                    var item = GameObject.Instantiate(this.item, transform);
                    item.name += i;
                    this.itemViewList.Add(item);
                    if (this.startSiblingIndex != null)
                    {
                        this.itemViewList[i].transform.SetSiblingIndex((int)this.startSiblingIndex + i);
                    }
                    item.SetActive(true);
                    this.onGetItemCallback?.Invoke(i, item);
                }
                else if (i < this.itemViewList.Count)
                {
                    this.itemViewList[i].SetActive(false);
                    if (this.startSiblingIndex != null)
                    {
                        this.itemViewList[i].transform.SetSiblingIndex((int)this.startSiblingIndex + i);
                    }
                }
            }
        }

        public void RefreshAllShownItem(int? startSiblingIndex = null)
        {
            if (startSiblingIndex != null)
            {
                this.startSiblingIndex = startSiblingIndex;
                if (this.startSiblingIndex > transform.childCount)
                {
                    this.startSiblingIndex = this.transform.childCount - 1;
                }
            }
            for (int i = 0; i < showCount; i++)
            {
                onGetItemCallback?.Invoke(i, this.itemViewList[i]);
            }
        }

        public GameObject GetItemByIndex(int index)
        {
            return itemViewList[index];
        }

        public int GetListItemCount()
        {
            return showCount;
        }
        
        public void Clear()
        {
            for (int i = itemViewList.Count - 1; i >= 0; i--)
            {
                Destroy(itemViewList[i]);
            }
            itemViewList.Clear();
        }
    }
}