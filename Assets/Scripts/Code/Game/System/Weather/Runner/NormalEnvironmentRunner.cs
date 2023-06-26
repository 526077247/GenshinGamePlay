namespace TaoTie
{
    public class NormalEnvironmentRunner: EnvironmentRunner
    {
        public virtual EnvironmentConfig Config { get; protected set; }
        
        public static NormalEnvironmentRunner Create(EnvironmentConfig data, EnvironmentPriorityType type, WeatherSystem weatherSystem)
        {
            var res = ObjectPool.Instance.Fetch<NormalEnvironmentRunner>();
            res.Priority = (int) type;
            res.Config = data;
            res.Id = IdGenerater.Instance.GenerateId();
            res.Data = EnvironmentInfo.Create(data);
            res.weatherSystem = weatherSystem;
            return res;
        }

        public override void Dispose()
        {
            // weatherSystem.RemoveFromMap(Id);
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