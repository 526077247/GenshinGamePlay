namespace TaoTie
{
    public enum StackingType
    {
        Unique, // 只能存在唯一一个
        Multiple, // 互相独立存在
        Refresh, // 刷新已存在的modifier
        Prolong, // 延长已存在的modifier
    }
}