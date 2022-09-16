namespace TaoTie
{
    public interface IOnDisable
    {
        public void OnDisable();
    }
    
    public interface IOnDisable<P1>
    {
        public void OnDisable(P1 p1);
    }

    public interface IOnDisable<P1, P2>
    {
        public void OnDisable(P1 p1,P2 p2);
    }
    
    public interface IOnDisable<P1,P2,P3>
    {
        public void OnDisable(P1 p1,P2 p2,P3 p3);
    }
    
    public interface IOnDisable<P1,P2,P3,P4>
    {
        public void OnDisable(P1 p1,P2 p2,P3 p3,P4 p4);
    }
}