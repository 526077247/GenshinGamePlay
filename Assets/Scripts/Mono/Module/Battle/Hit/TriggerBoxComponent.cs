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
    public class TriggerBoxComponent: MonoBehaviour
    {
        public List<Collider> TriggerList = new List<Collider>();

        public event Action<Collider> onTriggerEnterEvt;
        public event Action<Collider> onTriggerExitEvt;
        // public event Action<Collider> onTriggerStayEvt;

        public void OnTriggerEnter(Collider other)
        {
            TriggerList.Add(other);
            onTriggerEnterEvt?.Invoke(other);
        }
        
        public void OnTriggerExit(Collider other)
        {
            TriggerList.Remove(other);
            onTriggerExitEvt?.Invoke(other);
        }
        
        // public void OnTriggerStay(Collider other)
        // {
        //     onTriggerStayEvt?.Invoke(other);
        // }
    }
}