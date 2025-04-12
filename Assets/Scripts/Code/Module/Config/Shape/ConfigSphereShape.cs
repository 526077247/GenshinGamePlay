using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)] [LabelText("球")]
    public partial class ConfigSphereShape: ConfigShape
    {
        [NinoMember(1)]
        public float Radius;

        public override Collider CreateCollider(GameObject obj, bool isTrigger)
        {
            var collider = obj.AddComponent<SphereCollider>();
            collider.isTrigger = isTrigger;
            collider.radius = Radius;
            return collider;
        }

        public override bool Contains(Vector3 target)
        {
            return target.sqrMagnitude <= Radius * Radius;
        }
    }
}