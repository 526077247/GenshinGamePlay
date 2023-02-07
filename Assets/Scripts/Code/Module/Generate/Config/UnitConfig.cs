using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class UnitConfigCategory : ProtoObject, IMerge
    {
        public static UnitConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, UnitConfig> dict = new Dictionary<int, UnitConfig>();
        
        [NinoMember(1)]
        private List<UnitConfig> list = new List<UnitConfig>();
		
        public UnitConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            UnitConfigCategory s = o as UnitConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                UnitConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public UnitConfig Get(int id)
        {
            this.dict.TryGetValue(id, out UnitConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (UnitConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, UnitConfig> GetAll()
        {
            return this.dict;
        }
        public List<UnitConfig> GetAllList()
        {
            return this.list;
        }
        public UnitConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class UnitConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>Type</summary>
		[NinoMember(2)]
		public int Type { get; set; }
		/// <summary>名字</summary>
		[NinoMember(3)]
		public string Name { get; set; }
		/// <summary>描述</summary>
		[NinoMember(4)]
		public string Desc { get; set; }
		/// <summary>预制体路径</summary>
		[NinoMember(5)]
		public string Perfab { get; set; }
		/// <summary>身高</summary>
		[NinoMember(6)]
		public float Height { get; set; }

	}
}
