using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 标记用于触发判断
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ColliderBoxComponent: MonoBehaviour
    {
        public List<Collider> TriggerList = new List<Collider>();

        public event Action<Collider> OnTriggerEnterEvt;
        public event Action<Collider> OnTriggerExitEvt;
        // public event Action<Collider> OnTriggerStayEvt;
        public void OnTriggerEnter(Collider other)
        {
            TriggerList.Add(other);
            OnTriggerEnterEvt?.Invoke(other);
        }
        
        public void OnTriggerExit(Collider other)
        {
            TriggerList.Remove(other);
            OnTriggerExitEvt?.Invoke(other);
        }
        
        // public void OnTriggerStay(Collider other)
        // {
        //     OnTriggerStayEvt?.Invoke(other);
        // }
    }
}