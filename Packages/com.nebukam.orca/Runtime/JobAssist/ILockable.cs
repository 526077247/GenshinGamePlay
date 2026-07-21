namespace Nebukam.JobAssist
{
    public interface ILockable
    {
        bool locked { get; }
        void Lock();
        void Unlock();
    }
}
