namespace TaoTie
{
    public struct VariableChangeEvent: IEventBase
    {
        public string Key;
        public float OldValue;
        public float NewValue;
    }
}