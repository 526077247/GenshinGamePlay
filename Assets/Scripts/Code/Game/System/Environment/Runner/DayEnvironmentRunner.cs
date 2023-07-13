namespace TaoTie
{
    public class DayEnvironmentRunner:NormalEnvironmentRunner
    {
        private ConfigEnvironment[] datas;
        private int[] dayTimes;
        private int[] dayHalfInterval;
        private DayTimeType daytimetype = DayTimeType.None;
        private EasingFunction.Function lerpFunc;
        public static DayEnvironmentRunner Create(ConfigEnvironment morning,ConfigEnvironment noon,ConfigEnvironment afternoon,
            ConfigEnvironment night,EnvironmentPriorityType priority,EnvironmentManager environmentManager)
        {  
            DayEnvironmentRunner res = ObjectPool.Instance.Fetch<DayEnvironmentRunner>();
            res.environmentManager = environmentManager;
            res.Id = IdGenerater.Instance.GenerateId();
            res.datas = new[] {morning, noon, afternoon, night};
            res.Priority = (int)priority;
            res.dayTimes = new[]
            {
                environmentManager.MorningTimeStart,
                environmentManager.NoonTimeStart, 
                environmentManager.AfterNoonTimeStart,
                environmentManager.NightTimeStart
            };
            res.dayHalfInterval = new[]
            {
                environmentManager.DayTimeCount + environmentManager.NoonTimeStart -
                environmentManager.MorningTimeStart,
                environmentManager.DayTimeCount + environmentManager.AfterNoonTimeStart -
                environmentManager.NoonTimeStart,
                environmentManager.DayTimeCount + environmentManager.NightTimeStart -
                environmentManager.AfterNoonTimeStart,
                environmentManager.DayTimeCount + environmentManager.MorningTimeStart -
                environmentManager.NightTimeStart,
            };
            for (int i = 0; i < res.dayHalfInterval.Length; i++)
            {
                res.dayHalfInterval[i] %= environmentManager.DayTimeCount;
                res.dayHalfInterval[i] /= 2;
            }

            res.lerpFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
            res.UpdateDayTime();
            res.Data = EnvironmentInfo.Create(morning);
            res.LerpDatTime();
            res.IsOver = false;
            return res;
        }

        public override void Update()
        {
            Data.Changed = false;
            UpdateDayTime();
            LerpDatTime();
        }

        private bool UpdateDayTime()
        {
            DayTimeType nextType= DayTimeType.None;
            for (int i = dayTimes.Length-1; i >=0 ; i--)
            {
                if (environmentManager.NowTime>=dayTimes[i])
                {
                    nextType = (DayTimeType) i;
                    break;
                }
            }

            if (nextType != daytimetype)
            {
                daytimetype = nextType;
                Config = datas[(int) daytimetype];
                return true;
            }

            return false;
        }

        private void LerpDatTime()
        {
            var change = UpdateDayTime();
            int dayTimeIndex = (int) daytimetype;
            float progress = environmentManager.NowTime - dayTimes[dayTimeIndex];
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
            if(!change && datas[leftIndex].Id == datas[rightIndex].Id) return;
            if (end < start) end += environmentManager.DayTimeCount;
            progress %= dayHalfInterval[dayTimeIndex];
            var val = lerpFunc(progress / (end - start) / 2 + (ifLeftHalf ? 0.5f : 0), 0, 1);
            Data.Lerp(datas[leftIndex], datas[rightIndex], val);
            Data.Changed = true;
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
            lerpFunc = null;
            datas = default;
            dayTimes = default;
            dayHalfInterval = default;
            daytimetype = DayTimeType.None;
            ObjectPool.Instance.Recycle(this);
        }
    }
}