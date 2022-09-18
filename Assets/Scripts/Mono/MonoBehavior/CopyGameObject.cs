using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class CopyGameObject : MonoBehaviour
    {
        int? start_sibling_index;
        Action<int, GameObject> ongetitemcallback;
        int show_count;
        List<GameObject> m_itemviewlist = new List<GameObject>();
        public GameObject item;//要复制的物体
        public int InitCreateCount = 0;//初始化个数
        private void Awake()
        {
            this.start_sibling_index = item.transform.GetSiblingIndex();
            InitListView(InitCreateCount);
        }

        private void OnEnable()
        {
            item.SetActive(false);
        }

        public void InitListView(int total_count, Action<int, GameObject> ongetitemcallback = null, int? start_sibling_index = null)
        {
            //for (int i = 0; i < m_itemviewlist.Count; i++)
            //    Destroy(m_itemviewlist[i]);
            //m_itemviewlist.Clear();
            if (this.start_sibling_index != null)
            {
                this.start_sibling_index = start_sibling_index;
                if (this.start_sibling_index > transform.childCount)
                {
                    this.start_sibling_index = this.transform.childCount - 1;
                }
            }
            this.ongetitemcallback = ongetitemcallback;
            SetListItemCount(total_count);
        }

        public void SetListItemCount(int total_count, int? start_sibling_index = null)
        {
            if (total_count > 10) Debug.Log("total_count 不建议超过10个");
            if (item == null) Debug.LogError("item is Null!!!");
            if (this.start_sibling_index != null)
            {
                this.start_sibling_index = start_sibling_index;
                if (this.start_sibling_index > transform.childCount)
                {
                    this.start_sibling_index = this.transform.childCount - 1;
                }
            }
            this.show_count = total_count;
            var count = this.m_itemviewlist.Count > total_count ? this.m_itemviewlist.Count : total_count;
            for (int i = 0; i < count; i++)
            {
                if (i < this.m_itemviewlist.Count && i < total_count)
                {
                    this.m_itemviewlist[i].SetActive(true);
                    if (this.start_sibling_index != null)
                    {
                        this.m_itemviewlist[i].transform.SetSiblingIndex((int)this.start_sibling_index + i);
                    }
                    this.ongetitemcallback?.Invoke(i, this.m_itemviewlist[i]);
                }
                else if (i < total_count)
                {
                    var item = GameObject.Instantiate(this.item, transform);
                    this.m_itemviewlist.Add(item);
                    if (this.start_sibling_index != null)
                    {
                        this.m_itemviewlist[i].transform.SetSiblingIndex((int)this.start_sibling_index + i);
                    }
                    item.SetActive(true);
                    this.ongetitemcallback?.Invoke(i, item);
                }
                else if (i < this.m_itemviewlist.Count)
                {
                    this.m_itemviewlist[i].SetActive(false);
                    if (this.start_sibling_index != null)
                    {
                        this.m_itemviewlist[i].transform.SetSiblingIndex((int)this.start_sibling_index + i);
                    }
                }
            }
        }

        public void RefreshAllShownItem(int? start_sibling_index = null)
        {
            if (this.start_sibling_index != null)
            {
                this.start_sibling_index = start_sibling_index;
                if (this.start_sibling_index > transform.childCount)
                {
                    this.start_sibling_index = this.transform.childCount - 1;
                }
            }
            for (int i = 0; i < show_count; i++)
            {
                ongetitemcallback?.Invoke(i, this.m_itemviewlist[i]);
            }
        }

        public GameObject GetItemByIndex(int index)
        {
            return m_itemviewlist[index];
        }

        public int GetListItemCount()
        {
            return show_count;
        }
        
        public void Clear()
        {
            for (int i = m_itemviewlist.Count - 1; i >= 0; i--)
            {
                Destroy(m_itemviewlist[i]);
            }
            m_itemviewlist.Clear();
        }
    }
}