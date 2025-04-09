namespace TaoTie
{
    public abstract class Actor: Unit
    {
        /// <summary>
        /// 阵营id
        /// </summary>
        public uint CampId;
        
        public ConfigActor ConfigActor { get; protected set; }
    }
}