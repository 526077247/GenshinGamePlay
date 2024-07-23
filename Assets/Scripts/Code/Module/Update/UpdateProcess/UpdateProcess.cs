namespace TaoTie
{
    public abstract class UpdateProcess
    {
        public abstract ETTask<UpdateRes> Process(UpdateTask task);
        
    }
}