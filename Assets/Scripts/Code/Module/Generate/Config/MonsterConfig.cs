using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class MonsterConfigCategory : ProtoObject, IMerge
    {
        public static MonsterConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, MonsterConfig> dict = new Dictionary<int, MonsterConfig>();
        
        [NinoMember(1)]
        private List<MonsterConfig> list = new List<MonsterConfig>();
		
        public MonsterConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            MonsterConfigCategory s = o as MonsterConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                MonsterConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public MonsterConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MonsterConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MonsterConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MonsterConfig> GetAll()
        {
            return this.dict;
        }
        public List<MonsterConfig> GetAllList()
        {
            return this.list;
        }
        public MonsterConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class MonsterConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>模型Id</summary>
		[NinoMember(2)]
		public int UnitId { get; set; }
		/// <summary>AI路径</summary>
		[NinoMember(3)]
		public string AIPath { get; set; }

	}
}
