using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 出生点
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigBornByAttachPoint))]
    [ProtoInclude(101, typeof(ConfigBornBySelf))]
    [ProtoInclude(102, typeof(ConfigBornByTarget))]
    [ProtoInclude(103, typeof(ConfigBornByWorld))]
    public abstract partial class ConfigBornType
    {
        [ProtoMember(1, IsRequired = true)][NotNull][LabelText("坐标偏移")]
        public BaseVector3 PositionOffset = new ZeroVector3();
        [ProtoMember(2, IsRequired = true)][NotNull][LabelText("方向偏移")]
        public BaseVector3 RotationOffset = new ZeroVector3();

        public abstract Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public abstract Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public virtual async ETTask AfterBorn(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, Entity bornEntity)
        {
            await ETTask.CompletedTask;
        }
    }
}