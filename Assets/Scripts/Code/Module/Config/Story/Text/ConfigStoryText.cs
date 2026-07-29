using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigStoryContentText))]
    [ProtoInclude(101, typeof(ConfigStoryI18NText))]
    public abstract class ConfigStoryText
    {
        public abstract string GetShowText();
    }
}