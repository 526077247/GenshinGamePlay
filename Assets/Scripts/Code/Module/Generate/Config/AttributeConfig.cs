using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    [Obfuz.ObfuzIgnore]
    public partial class AttributeConfigCategory : ProtoObject, IMerge
    {
        public static AttributeConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, AttributeConfig> dict = new Dictionary<int, AttributeConfig>();
        
        [NinoMember(1)]
        private List<AttributeConfig> list = new List<AttributeConfig>();
		
        public AttributeConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            AttributeConfigCategory s = o as AttributeConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                AttributeConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public AttributeConfig Get(int id)
        {
            this.dict.TryGetValue(id, out AttributeConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (AttributeConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, AttributeConfig> GetAll()
        {
            return this.dict;
        }
        public List<AttributeConfig> GetAllList()
        {
            return this.list;
        }
        public AttributeConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class AttributeConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>索引</summary>
		[NinoMember(2)]
		public string Key { get; set; }
		/// <summary>名称</summary>
		[NinoMember(3)]
		public string Name { get; set; }
		/// <summary>类型(0:整数,1:小数)</summary>
		[NinoMember(4)]
		public int Type { get; set; }
		/// <summary>显示</summary>
		[NinoMember(5)]
		public int Show { get; set; }
		/// <summary>是否被BUFF影响</summary>
		[NinoMember(6)]
		public int Affected { get; set; }
		/// <summary>最大值</summary>
		[NinoMember(7)]
		public string MaxAttr { get; set; }
		/// <summary>回复值</summary>
		[NinoMember(8)]
		public string AttrReUp { get; set; }
		/// <summary>描述</summary>
		[NinoMember(9)]
		public string Desc { get; set; }

	}
}
