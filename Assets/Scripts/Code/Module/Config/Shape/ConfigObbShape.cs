using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)] [LabelText("立方")]
    public partial class ConfigObbShape: ConfigShape
    {
        [NinoMember(1)]
        public Vector3 Size;

        public override Collider CreateCollider(GameObject obj, bool isTrigger)
        {
            var collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = isTrigger;
            collider.size = Size;
            return collider;
        }

        public override bool Contains(Vector3 target)
        {
            var x = Size.x / 2;
            var y = Size.y / 2;
            var z = Size.z / 2;
            return -x <= target.x && target.x <= x
                                  && -y <= target.y && target.y <= y
                                  && -z < target.z && target.z < z;
        }
    }
}