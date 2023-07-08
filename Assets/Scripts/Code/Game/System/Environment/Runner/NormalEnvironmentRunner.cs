namespace TaoTie
{
    public class NormalEnvironmentRunner: EnvironmentRunner
    {
        public virtual ConfigEnvironment Config { get; protected set; }
        
        public static NormalEnvironmentRunner Create(ConfigEnvironment data, EnvironmentPriorityType type, EnvironmentManager environmentManager)
        {
            var res = ObjectPool.Instance.Fetch<NormalEnvironmentRunner>();
            res.Priority = (int) type;
            res.Config = data;
            res.Id = IdGenerater.Instance.GenerateId();
            res.Data = EnvironmentInfo.Create(data);
            res.environmentManager = environmentManager;
            res.IsOver = false;
            return res;
        }

        public override void Dispose()
        {
            environmentManager.RemoveFromMap(Id);
            //base
            Id = default;
            Priority = default;
            IsOver = true;
            Data?.Dispose();
            Data = null;
            //this
            Config = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}