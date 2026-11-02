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
		
		private Dictionary<int, long> NumericDic;

		private long this[int numericType]
		{
			get
			{
				return this.GetByKey(numericType);
			}
			set
			{
				if (numericType < NumericType.Max)
				{
					Log.Error("不允许直接设置最终值，请检查代码");
					numericType = numericType * 10 + 1;
				}
				this.Insert(numericType,value);
			}
		}
		
		/// <summary>
		/// 读真实值。小数属性存的是 值*10000，这里还原；整数属性原样返回。
		/// </summary>
		public float GetAsFloat(int numericType)
		{
			long raw = this.GetByKey(numericType);
			if (this.IsFloat(numericType))
				return raw / 10000f;
			return raw;
		}

		public int GetAsInt(int numericType)
		{
			if(this.IsFloat(numericType))
				return (int)(this.GetByKey(numericType)/10000);
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

		/// <summary>
		/// 设置值
		/// </summary>
		/// <param name="nt"> nt>NumericType.Max </param>
		/// <param name="value"></param>
		/// <param name="isRealValue"></param>
		public void Set(int nt, float value, bool isRealValue = false)
		{
			if (!isRealValue && this.IsFloat(nt))
				this[nt] = (int) (value * 10000);
			else
				this[nt] = (int) value;
		}

		/// <summary>
		/// 设置值
		/// </summary>
		/// <param name="nt"> nt>NumericType.Max </param>
		/// <param name="value"></param>
		/// <param name="isRealValue"></param>
		public void Set(int nt, int value, bool isRealValue = false)
		{
			if (!isRealValue && this.IsFloat(nt))
				this[nt] = value * 10000;
			else
				this[nt] = value;
		}

		/// <summary>
		/// 设置值
		/// </summary>
		/// <param name="nt"> nt>NumericType.Max </param>
		/// <param name="value"></param>
		/// <param name="isRealValue"></param>
		public void Set(int nt, long value, bool isRealValue = false)
		{
			if (!isRealValue && this.IsFloat(nt))
				this[nt] = value * 10000;
			else
				this[nt] = value;
		}

		public void SetNoEvent(int numericType, long value)
		{
			this.Insert(numericType,value,false);
		}

		#region 原始值读写（给"成对加减"用）

		/// <summary>
		/// 读原始存储值，不做 float 还原。整数属性就是值本身，小数属性是 值*10000。
		/// 装备加成这类"记下我加了多少、下次先减再加"的场景必须用它，
		/// 否则 GetAsFloat → Set 的往返会在小数段产生 ±1/10000 漂移，反复刷新后越差越多。
		/// </summary>
		public long GetRaw(int numericType)
		{
			return this.GetByKey(numericType);
		}

		/// <summary>
		/// 写原始存储值。会正常触发五段式重算与 NumericChangeEvt。
		/// <paramref name="numericType"/> 必须是派生段（&gt;= <see cref="NumericType.Max"/>），
		/// 填基础段会被索引器改成 Base 段并报错。
		/// </summary>
		public void SetRaw(int numericType, long value)
		{
			if (numericType < NumericType.Max)
			{
				Log.Error($"SetRaw 不允许设置基础段 id={numericType}，请用派生段（>= {NumericType.Max}）");
				return;
			}
			this[numericType] = value;
		}

		/// <summary>
		/// float 配置值 → 原始存储值。小数段乘 10000 后**四舍五入**（不用截断：
		/// 20f * 1.0000001f * 10000 = 200000.02，截断是安全的，但 19.999999 * 10000 = 199999.99 截断会掉 1）。
		/// </summary>
		public long ToRaw(int numericType, float value)
		{
			if (numericType < NumericType.Max)
			{
				Log.Error($"ToRaw 不允许使用基础段 id={numericType}，请用派生段（>= {NumericType.Max}）");
				return 0;
			}
			if (this.IsFloat(numericType)) return (long) System.Math.Round(value * 10000f);
			return (long) System.Math.Round(value);
		}

		#endregion
		
		private void Insert(int numericType, long value,bool isPublicEvent = true)
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
				args.Parent = this.parent;
				args.NumericType = numericType;
				args.OldRaw = oldValue;
				args.NewRaw = value;
				// Raw 与 Real 一次性算好，避免每个订阅者各自查表换算
				if (this.IsFloat(numericType))
				{
					args.OldReal = oldValue / 10000f;
					args.NewReal = value / 10000f;
				}
				else
				{
					args.OldReal = oldValue;
					args.NewReal = value;
				}
				Messager.Instance.Broadcast(Id,MessageId.NumericChangeEvt,args);
			}
		}
		
		private long GetByKey(int key)
		{
			long value = 0;
			this.NumericDic.TryGetValue(key, out value);
			return value;
		}

		private void Update(int numericType,bool isPublicEvent)
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
		/// 读表判断是取整还是保留小数。
		/// 外部写入前需要据此决定取整策略：<see cref="Set(int,float,bool)"/> 对整数属性是
		/// <c>(int)value</c> 向零截断，24.5 会变成 24，和"四舍五入"的直觉不符。
		/// </summary>
		internal bool IsFloat(int numericType)
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