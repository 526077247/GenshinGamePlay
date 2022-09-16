namespace TaoTie
{
    public interface IOnEnable
    {
        public void OnEnable();
    }
    
    public interface IOnEnable<P1>
    {
        public void OnEnable(P1 p1);
    }

    public interface IOnEnable<P1, P2>
    {
        public void OnEnable(P1 p1,P2 p2);
    }
    
    public interface IOnEnable<P1,P2,P3>
    {
        public void OnEnable(P1 p1,P2 p2,P3 p3);
    }
    
    public interface IOnEnable<P1,P2,P3,P4>
    {
        public void OnEnable(P1 p1,P2 p2,P3 p3,P4 p4);
    }
}