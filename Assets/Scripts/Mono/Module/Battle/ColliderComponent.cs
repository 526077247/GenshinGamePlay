using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ColliderComponent : MonoBehaviour
    {
        public Action<long, bool> OnEntityTrigger;
        private void OnTriggerEnter(Collider other)
        {
            var ec = other.GetComponent<EntityComponent>();
            if (ec != null)
            {
                OnEntityTrigger?.Invoke(ec.Id, true);
            }
            
        }
        
        private void OnTriggerExit(Collider other)
        {
            var ec = other.GetComponent<EntityComponent>();
            if (ec != null)
            {
                OnEntityTrigger?.Invoke(ec.Id, false);
            }
        }
    }
}
