using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [LabelText("Debug/*显示攻击范围")][Tooltip("运行时显示攻击范围，用于调试")]
    public partial class ShowRangeAction : ConfigAbilityAction
    {
        [NotNull] [ProtoMember(10, IsRequired = true)]
        public ConfigAttackEvent AttackEvent = new ConfigAttackEvent();

        [ProtoMember(11)][LabelText("显示时长(ms)")][MinValue(100)]
        public int Duration = 2000;

        [ProtoMember(12)][LabelText("显示颜色")]
        public Color RangeColor = new Color(1f, 0.2f, 0.2f, 0.3f);

        protected override void Execute(Entity actionExecuter, ActorAbility ability,
            ActorModifier modifier, Entity target)
        {
            if (AttackEvent?.AttackPattern == null) return;

            var pattern = AttackEvent.AttackPattern;

            if (pattern is ConfigAttackBox box)
            {
                var pos = box.Born.ResolvePos(actionExecuter, ability, modifier, target);
                var rot = box.Born.ResolveRot(actionExecuter, ability, modifier, target);
                var size = box.Size.Resolve(actionExecuter, ability);
                CreateBoxPreview(pos, rot, size);
            }
            else if (pattern is ConfigAttackSphere sphere)
            {
                var pos = sphere.Born.ResolvePos(actionExecuter, ability, modifier, target);
                var radius = sphere.Radius.Resolve(actionExecuter, ability);
                CreateSpherePreview(pos, radius);
            }
        }

        private void CreateBoxPreview(Vector3 pos, Quaternion rot, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.transform.localScale = size;
            ApplyMaterial(go);
            Object.Destroy(go, Duration / 1000f);
        }

        private void CreateSpherePreview(Vector3 pos, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * radius * 2;
            ApplyMaterial(go);
            Object.Destroy(go, Duration / 1000f);
        }

        private void ApplyMaterial(GameObject go)
        {
            Object.Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("TaoTie RP/Unlit"))
            {
                color = RangeColor
            };
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            renderer.material = mat;
        }
    }
}
