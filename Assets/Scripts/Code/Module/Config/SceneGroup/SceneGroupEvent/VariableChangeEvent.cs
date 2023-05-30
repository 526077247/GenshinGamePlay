namespace TaoTie
{
    public class VariableChangeEvent: IEventBase
    {
        public string Key;
        public float OldValue;
        public float NewValue;
    }
}