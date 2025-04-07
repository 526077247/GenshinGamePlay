using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 标记用于触发判断
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerBoxComponent: MonoBehaviour
    {
        public List<Collider> TriggerList = new List<Collider>();
        public void OnTriggerEnter(Collider other)
        {
            TriggerList.Add(other);
        }
        
        public void OnTriggerExit(Collider other)
        {
            TriggerList.Remove(other);
        }
    }
}