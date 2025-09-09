using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TaoTie
{
    public class UISlider : UIBaseContainer,IOnDestroy
    {
        private Slider slider;
        private UnityAction<float> onValueChanged;
        private bool isWholeNumbers;
        private ArrayList valueList;

        #region override

        public void OnDestroy()
        {
            this.RemoveOnValueChanged();
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.slider == null)
            {
                this.slider = this.GetGameObject().GetComponent<Slider>();
                if (this.slider == null)
                {
                    Log.Error($"添加UI侧组件UISlider时，物体{this.GetGameObject().name}上没有找到Slider组件");
                }
            }
        }
        public void SetOnValueChanged(UnityAction<float> callback)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.onValueChanged = callback;
            this.slider.onValueChanged.AddListener(this.onValueChanged);
        }

        public void RemoveOnValueChanged()
        {
            if (this.onValueChanged != null)
            {
                this.slider.onValueChanged.RemoveListener(this.onValueChanged);
                this.onValueChanged = null;
            }
        }

        public void SetWholeNumbers(bool wholeNumbers)
        {
            this.ActivatingComponent();
            this.slider.wholeNumbers = wholeNumbers;
            this.isWholeNumbers = wholeNumbers;
        }

        public void SetValueList(ArrayList valueList)
        {
            this.valueList = valueList;
            this.SetWholeNumbers(true);
            this.SetMinValue(0);
            this.SetMaxValue(valueList.Count - 1);
        }
   
        public ArrayList GetValueList()
        {
            return this.valueList;
        }
        
        public void SetWholeNumbersValue(object value)
        {
            this.ActivatingComponent();
            if (!this.isWholeNumbers)
            {
                Log.Warning("请先设置WholeNumbers为true");
                return;
            }

            for (int i = 0; i < this.valueList.Count; i++)
            {
                if (this.valueList[i] == value)
                {
                    this.slider.value = i;
                    return;
                }
            }
        }
        
        public object GetWholeNumbersValue()
        {
            this.ActivatingComponent();
            if (!this.isWholeNumbers)
            {
                Log.Warning("请先设置WholeNumbers为true");
                return default;
            }
            var index = (int)this.slider.value;
            return this.valueList[index];
        }
        
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public void SetValue(float value)
        {
            this.ActivatingComponent();
            if (!this.isWholeNumbers)
            {
                this.slider.value = value;
            }
            else
            {
                this.slider.value = (int)value;
            }
        }

        /// <summary>
        /// 获取进度
        /// </summary>
        /// <returns>wholeNumbers 时value是ui侧的index</returns>
        public float GetValue()
        {
            this.ActivatingComponent();
            return this.slider.value;
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public void SetNormalizedValue(float value)
        {
            this.ActivatingComponent();
            this.slider.normalizedValue = value;
        }
        
        public float GetNormalizedValue()
        {
            this.ActivatingComponent();
            return this.slider.normalizedValue;
        }
        
        public void SetMaxValue(float value)
        {
            this.ActivatingComponent();
            this.slider.maxValue = value;
        }

        public void SetMinValue(float value)
        {
            this.ActivatingComponent();
            this.slider.minValue = value;
        }
    }
}
