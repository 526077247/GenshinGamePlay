using System.Collections.Generic;

namespace TaoTie
{
	public class NumericComponent:Component,IComponent,IComponent<ConfigCombatProperty[]>
	{
		#region override
		public void Init()
		{
			NumericDic = new Dictionary<int, long>();
			ManagerProvider.RegisterManager<NumericSystem>().AddComponent(this);
		}
		public void Init(ConfigCombatProperty[] list)
		{
			NumericDic = new Dictionary<int, long>();
			ManagerProvider.RegisterManager<NumericSystem>().AddComponent(this);
			if (list != null)
			{
				for (int i = 0; i < list.Length; i++)
				{
					Set(list[i].NumericType,list[i].Value);
				}
			}
		}
		public void Destroy()
		{
			ManagerProvider.RegisterManager<NumericSystem>().RemoveComponent(this);
			NumericDic.Clear();
			NumericDic = null;
		}

		#endregion
		
		public Dictionary<int, long> NumericDic;

		public long this[int numericType]
		{
			get
			{
				return this.GetByKey(numericType);
			}
			set
			{
				this.Insert(numericType,value);
			}
		}
		
		public float GetAsFloat(int numericType)
		{
			if(this.IsFloat(numericType))
				return this.GetByKey(numericType)/10000f;
			else
				return this.GetByKey(numericType);
		}

		public int GetAsInt(int numericType)
		{
			if(this.IsFloat(numericType))
				return (int)this.GetByKey(numericType)/10000;
			else
				return (int)this.GetByKey(numericType);
		}
		
		public long GetAsLong(int numericType)
		{
			if(this.IsFloat(numericType))
				return this.GetByKey(numericType)/10000;
			else
				return this.GetByKey(numericType);
		}

		public void Set(int nt, float value,bool isRealValue=false)
		{
			if(!isRealValue&&this.IsFloat(nt))
				this[nt] = (int) (value * 10000);
			else
				this[nt] = (int) value;
		}

		public void Set(int nt, int value,bool isRealValue=false)
		{
			if(!isRealValue&&this.IsFloat(nt))
				this[nt] = value * 10000;
			else
				this[nt] = value;
		}
		
		public void Set(int nt, long value,bool isRealValue=false)
		{
			if(!isRealValue&&this.IsFloat(nt))
				this[nt] = value * 10000;
			else
				this[nt] = value;
		}

		public void SetNoEvent(int numericType, long value)
		{
			this.Insert(numericType,value,false);
		}
		
		public void Insert(int numericType, long value,bool isPublicEvent = true)
		{
			long oldValue = this.GetByKey(numericType);
			if (oldValue == value)
			{
				return;
			}

			this.NumericDic[numericType] = value;

			if (numericType >= NumericType.Max)
			{
				this.Update(numericType,isPublicEvent);
				return;
			}

			if (isPublicEvent)
			{
				NumericChange args = new NumericChange();
				args.Parent = this.Parent;
				args.NumericType = numericType;
				args.Old = oldValue;
				args.New = value;
				Messager.Instance.Broadcast(Id,MessageId.NumericChangeEvt,args);
			}
		}
		
		private long GetByKey(int key)
		{
			long value = 0;
			this.NumericDic.TryGetValue(key, out value);
			return value;
		}

		public void Update(int numericType,bool isPublicEvent)
		{
			int final = (int) numericType / 10;
			int bas = final * 10 + 1; 
			int add = final * 10 + 2;
			int pct = final * 10 + 3;
			int finalAdd = final * 10 + 4;
			int finalPct = final * 10 + 5;

			// 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
			// final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
			long result = (long)(((this.GetByKey(bas) + this.GetByKey(add)) * (100 + this.GetAsFloat(pct)) / 100f + this.GetByKey(finalAdd)) * (100 + this.GetAsFloat(finalPct)) / 100f);
			this.Insert(final,result,isPublicEvent);
		}
		
		/// <summary>
		/// 读表判断是取整还是保留小数
		/// </summary>
		/// <param name="numericType"></param>
		/// <returns></returns>
		public bool IsFloat(int numericType)
		{
			if (numericType > NumericType.Max)
			{
				var flag = numericType % 10;
				if (flag == 3 || flag == 5) return true;//百分比的是小数,否则看配置表
				numericType /= 10;
			}
			var attr = AttributeConfigCategory.Instance.Get(numericType);
			return attr.Type == 1;
		}
	}
	
}