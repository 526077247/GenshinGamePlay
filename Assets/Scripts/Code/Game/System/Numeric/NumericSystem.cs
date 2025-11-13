using System.Collections.Generic;

namespace TaoTie
{
    public class NumericSystem:IManager,IUpdate
    {
        private struct ReUpAttr
        {
            public int attr;
            public int reUp;
            public int max;
        }
        /// <summary>
        /// 数值变化检测间隔
        /// </summary>
        private const int ATTRCHANGE_CHECKTIME = 250;
        /// <summary>
        /// 数值变化间隔
        /// </summary>
        private const int ATTRCHANGE_DELTATIME = 1000;
        
        public List<NumericComponent> Data;
        private long Timer;
        
        private ReUpAttr[] attrList;

        #region override
        
        [Timer(TimerType.NumericUpdate)]
        public class NumericUpdateTimer:ATimer<NumericSystem>
        {
            public override void Run(NumericSystem t)
            {
                t.NumericUpdate();
            }
        }

        public void Init()
        {
            using ListComponent<ReUpAttr> attrList = ListComponent<ReUpAttr>.Create();
            var attrs = AttributeConfigCategory.Instance.GetAllList();
            for (int i = 0; i < attrs.Count; i++)
            {
                if (!string.IsNullOrEmpty(attrs[i].AttrReUp) && !string.IsNullOrEmpty(attrs[i].MaxAttr))
                {
                    var reup = NumericType.GetKey(attrs[i].AttrReUp);
                    var max = NumericType.GetKey(attrs[i].MaxAttr);
                    if (reup >= 0 && max>=0)
                    {
                        attrList.Add(new ReUpAttr()
                        {
                            attr = attrs[i].Id * 10 + 1,
                            reUp = reup * 10 + 1,
                            max = max * 10 + 1,
                        });
                    }
                }
            }

            this.attrList = attrList.ToArray();
            Data = new List<NumericComponent>();
        }

        public void Destroy()
        {
            GameTimerManager.Instance?.Remove(ref Timer);
            Data.Clear();
            Data = null;
        }

        public void Update()
        {
            if (GameTimerManager.Instance == null)
            {
                Timer = 0;
            }
            if (Timer == 0 && GameTimerManager.Instance != null)
            {
                Timer = GameTimerManager.Instance.NewRepeatedTimer(ATTRCHANGE_CHECKTIME, TimerType.NumericUpdate, this);
            }
        }

        public void NumericUpdate()
        {
            //遍历回血，回蓝
            for (int i = 0; i < Data.Count; i++)
            {
                var numc = Data[i];
                for (int j = 0; j < attrList.Length; j++)
                {
                    float reUpNum = numc.GetAsFloat(attrList[j].reUp);
                    reUpNum = reUpNum * ATTRCHANGE_CHECKTIME / ATTRCHANGE_DELTATIME;
                    if (reUpNum > 0)
                    {
                        var maxValue = numc.GetAsInt(attrList[j].max);
                        float nowValue = numc.GetAsInt(attrList[j].attr);
                        if (nowValue < maxValue)
                        {
                            nowValue += reUpNum;
                            if (nowValue > maxValue) nowValue = maxValue;
                            numc.Set(attrList[j].attr, nowValue);
                        }
                    }
                }
            }

        }

        #endregion

        public void AddComponent(NumericComponent component)
        {
            Data.Add(component);
        }

        public void RemoveComponent(NumericComponent component)
        {
            Data.Remove(component);
        }
    }
}