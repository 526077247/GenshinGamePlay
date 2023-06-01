namespace TaoTie
{
    public struct AttackTarget
    {
        public long RuntimeID;
        public string LockedPoint;

        public void Reset()
        {
            RuntimeID = 0;
            LockedPoint = null;
        }
    }
}