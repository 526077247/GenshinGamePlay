namespace TaoTie
{
    public interface IOnCreate
    {
        public void OnCreate();
    }
    
    public interface IOnCreate<P1>
    {
        public void OnCreate(P1 p1);
    }

    public interface IOnCreate<P1, P2>
    {
        public void OnCreate(P1 p1,P2 p2);
    }
    
    public interface IOnCreate<P1,P2,P3>
    {
        public void OnCreate(P1 p1,P2 p2,P3 p3);
    }
    
    public interface IOnCreate<P1,P2,P3,P4>
    {
        public void OnCreate(P1 p1,P2 p2,P3 p3,P4 p4);
    }
}