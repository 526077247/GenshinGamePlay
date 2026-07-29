using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(OperatorSceneGroupValue))]
    [ProtoInclude(101, typeof(SceneGroupValue))]
    [ProtoInclude(102, typeof(SingleSceneGroupValue))]
    public abstract partial class BaseSceneGroupValue
    {
        public abstract float Resolve(IEventBase obj, DynDictionary set);
    }
}