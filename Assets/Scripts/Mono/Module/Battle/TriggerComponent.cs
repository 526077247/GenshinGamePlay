using System;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public class TriggerComponent: MonoBehaviour
    {
        public EntityType CastEntityType;
        public UnityAction<long,TriggerType,Vector3> OnTriggerEnterEvt;
        public UnityAction<long> OnTriggerStayEvt;
        public UnityAction<long,TriggerType,Vector3> OnTriggerExitEvt;
        private void OnTriggerEnter(Collider other)
        {
            var entity = other.GetComponentInParent<EntityComponent>();
            if (entity.EntityType == CastEntityType||CastEntityType == EntityType.ALL)
            {
                var hitPos = other.bounds.ClosestPoint(transform.position);
                OnTriggerEnterEvt?.Invoke(entity.Id,TriggerType.Enter,hitPos);
            }
           
        }
        private void OnTriggerStay(Collider other)
        {
            var entity = other.GetComponentInParent<EntityComponent>();
            if (entity.EntityType == CastEntityType||CastEntityType == EntityType.ALL)
            {
                OnTriggerStayEvt?.Invoke(entity.Id);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            var entity = other.GetComponentInParent<EntityComponent>();
            if (entity.EntityType == CastEntityType||CastEntityType == EntityType.ALL)
            {
                var hitPos = other.bounds.ClosestPoint(transform.position);
                OnTriggerExitEvt?.Invoke(entity.Id,TriggerType.Exit,hitPos);
            }
        }
    }
}