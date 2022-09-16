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
        public Slider unity_uislider;
        public UnityAction<float> __onValueChanged;
        public bool isWholeNumbers;
        public ArrayList value_list;

        #region override

        public void OnDestroy()
        {
            this.RemoveOnValueChanged();
        }

        #endregion
        
        void ActivatingComponent()
        {
            if (this.unity_uislider == null)
            {
                this.unity_uislider = this.GetGameObject().GetComponent<Slider>();
                if (this.unity_uislider == null)
                {
                    Log.Error($"添加UI侧组件UISlider时，物体{this.GetGameObject().name}上没有找到Slider组件");
                }
            }
        }
        public void SetOnValueChanged(UnityAction<float> callback)
        {
            this.ActivatingComponent();
            this.RemoveOnValueChanged();
            this.__onValueChanged = callback;
            this.unity_uislider.onValueChanged.AddListener(this.__onValueChanged);
        }

        public void RemoveOnValueChanged()
        {
            if (this.__onValueChanged != null)
            {
                this.unity_uislider.onValueChanged.RemoveListener(this.__onValueChanged);
                this.__onValueChanged = null;
            }
        }

        public void SetWholeNumbers(bool wholeNumbers)
        {
            this.ActivatingComponent();
            this.unity_uislider.wholeNumbers = wholeNumbers;
            this.isWholeNumbers = true;
        }

        public void SetMaxValue(float value)
        {
            this.ActivatingComponent();
            this.unity_uislider.maxValue = value;
        }

        public void SetMinValue(float value)
        {
            this.ActivatingComponent();
            this.unity_uislider.minValue = value;
        }

        public void SetValueList(ArrayList value_list)
        {
            this.value_list = value_list;
            this.SetWholeNumbers(true);
            this.SetMinValue(0);
            this.SetMaxValue(value_list.Count - 1);
        }
   
        public ArrayList GetValueList()
        {
            return this.value_list;
        }

        public object GetValue()
        {
            this.ActivatingComponent();
            if (this.isWholeNumbers)
            {
                var index = (int)this.unity_uislider.value;
                return this.value_list[index];
            }
            else
            {
                return this.unity_uislider.normalizedValue;
            }
        }
        public object GetNormalizedValue()
        {
            this.ActivatingComponent();
            return this.unity_uislider.normalizedValue;
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public void SetValue(int value)
        {
            this.ActivatingComponent();
            this.unity_uislider.value = value;
        }
        
        public void SetWholeNumbersValue(object value)
        {
            this.ActivatingComponent();
            if (!this.isWholeNumbers)
            {
                Log.Warning("请先设置WholeNumbers为true");
                return;
            }

            for (int i = 0; i < this.value_list.Count; i++)
            {
                if (this.value_list[i] == value)
                {
                    this.unity_uislider.value = i;
                    return;
                }
            }
            
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public void SetNormalizedValue(float value)
        {
            this.ActivatingComponent();
            this.unity_uislider.normalizedValue = value;
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(float value)
        {
            this.ActivatingComponent();
            if (!this.isWholeNumbers)
                this.unity_uislider.value = value;
            else
            {
                Log.Warning("请先设置WholeNumbers为false");
                this.unity_uislider.value = (int)value;
            }
        }
    }
}
