using System;
using System.Collections.Generic;
using ProtoBuf;

using System.Numerics;
namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class ElementConfigCategory : ProtoObject, IMerge
    {
        public static ElementConfigCategory Instance => ConfigManager.GetConfig<ElementConfigCategory>();

        
        [ProtoIgnore]
        private Dictionary<int, ElementConfig> dict = new Dictionary<int, ElementConfig>();
        
        [ProtoMember(1)]
        private List<ElementConfig> list = new List<ElementConfig>();
		
        public ElementConfigCategory()
        {
        }
        
        public void Merge(object o)
        {
            ElementConfigCategory s = o as ElementConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                ElementConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public ElementConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ElementConfig item);

            if (item == null)
            {
#if !NOT_UNITY
                Log.Error($"配置找不到，配置表名: {nameof (ElementConfig)}，配置id: {id}");
#endif
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ElementConfig> GetAll()
        {
            return this.dict;
        }
        public List<ElementConfig> GetAllList()
        {
            return this.list;
        }
        public ElementConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class ElementConfig: ProtoObject
	{
		/// <summary>ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名字（只用于日志与表现，代码只认 Id）</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>耐久上限（元素量封顶，整数；0 = 该元素不可附着）</summary>
		[ProtoMember(3)]
		public int MaxDurability { get; set; }
		/// <summary>每秒自然衰减（按游戏时间；0 = 永不自然消失）</summary>
		[ProtoMember(4)]
		public float DecayPerSecond { get; set; }

	}
}
