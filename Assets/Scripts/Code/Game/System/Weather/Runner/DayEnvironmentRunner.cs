namespace TaoTie
{
    public class DayEnvironmentRunner:NormalEnvironmentRunner
    {
        private EnvironmentConfig[] datas;
        private int[] dayTimes;
        private int[] dayHalfInterval;
        private DayTimeType daytimetype = DayTimeType.None;
        private EasingFunction.Function lerpFunc;
        public static DayEnvironmentRunner Create(EnvironmentConfig morning,EnvironmentConfig noon,EnvironmentConfig afternoon,
            EnvironmentConfig night,EnvironmentPriorityType priority,WeatherSystem weatherSystem)
        {  
            DayEnvironmentRunner res = ObjectPool.Instance.Fetch<DayEnvironmentRunner>();
            res.weatherSystem = weatherSystem;
            res.Id = IdGenerater.Instance.GenerateId();
            res.datas = new[] {morning, noon, afternoon, night};
            res.Priority = (int)priority;
            res.dayTimes = new[]
            {
                weatherSystem.MorningTimeStart,
                weatherSystem.NoonTimeStart, 
                weatherSystem.AfterNoonTimeStart,
                weatherSystem.NightTimeStart
            };
            res.dayHalfInterval = new[]
            {
                weatherSystem.DayTimeCount + weatherSystem.NoonTimeStart -
                weatherSystem.MorningTimeStart,
                weatherSystem.DayTimeCount + weatherSystem.AfterNoonTimeStart -
                weatherSystem.NoonTimeStart,
                weatherSystem.DayTimeCount + weatherSystem.NightTimeStart -
                weatherSystem.AfterNoonTimeStart,
                weatherSystem.DayTimeCount + weatherSystem.MorningTimeStart -
                weatherSystem.NightTimeStart,
            };
            for (int i = 0; i < res.dayHalfInterval.Length; i++)
            {
                res.dayHalfInterval[i] %= weatherSystem.DayTimeCount;
                res.dayHalfInterval[i] /= 2;
            }

            res.lerpFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
            res.UpdateDayTime();
            res.Data = EnvironmentInfo.Create(res.Config);
            res.LerpDatTime();
            return res;
        }

        public override void Update()
        {
            Data.Changed = false;
            UpdateDayTime();
            LerpDatTime();
        }

        private void UpdateDayTime()
        {
            DayTimeType nextType= DayTimeType.None;
            for (int i = dayTimes.Length-1; i >=0 ; i--)
            {
                if (weatherSystem.NowTime>=dayTimes[i])
                {
                    nextType = (DayTimeType) i;
                    break;
                }
            }

            if (nextType != daytimetype)
            {
                daytimetype = nextType;
                Config = datas[(int) daytimetype];
            }
        }

        private void LerpDatTime()
        {
            int dayTimeIndex = (int) daytimetype;
            float progress = weatherSystem.NowTime - dayTimes[dayTimeIndex];
            var ifLeftHalf = progress < dayHalfInterval[dayTimeIndex];
            int leftIndex;
            int rightIndex;
            int start;
            int end;
            if (ifLeftHalf)
            {
                leftIndex = (dayTimeIndex - 1 + dayTimes.Length) % dayTimes.Length;
                rightIndex = dayTimeIndex;
                start = dayTimes[rightIndex];
                end = dayTimes[rightIndex] + dayHalfInterval[rightIndex];
            }
            else
            {
                leftIndex = dayTimeIndex;
                rightIndex= (dayTimeIndex + 1) % dayTimes.Length;
                start = dayTimes[leftIndex] + dayHalfInterval[leftIndex];
                end = dayTimes[rightIndex];
            }
            if(datas[leftIndex].Id == datas[rightIndex].Id) return;
            progress %= dayHalfInterval[dayTimeIndex];
            var val = lerpFunc((progress-start)/(end-start),0,1);
            Data.Lerp(datas[leftIndex], datas[rightIndex], val);
            Data.Changed = true;
        }

        public override void Dispose()
        {
            weatherSystem.RemoveFromMap(Id);
            //base
            Id = default;
            Priority = default;
            IsOver = true;
            Data?.Dispose();
            Data = null;
            //this
            Config = null;
            lerpFunc = null;
            datas = default;
            dayTimes = default;
            dayHalfInterval = default;
            daytimetype = DayTimeType.None;
            ObjectPool.Instance.Recycle(this);
        }
    }
}