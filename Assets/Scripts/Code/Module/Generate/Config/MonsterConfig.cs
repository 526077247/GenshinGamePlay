using System;
using System.Collections.Generic;
using ProtoBuf;

using System.Numerics;
namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class MonsterConfigCategory : ProtoObject, IMerge
    {
        public static MonsterConfigCategory Instance => ConfigManager.GetConfig<MonsterConfigCategory>();

        
        [ProtoIgnore]
        private Dictionary<int, MonsterConfig> dict = new Dictionary<int, MonsterConfig>();
        
        [ProtoMember(1)]
        private List<MonsterConfig> list = new List<MonsterConfig>();
		
        public MonsterConfigCategory()
        {
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
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public MonsterConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MonsterConfig item);

            if (item == null)
            {
#if !NOT_UNITY
                Log.Error($"配置找不到，配置表名: {nameof (MonsterConfig)}，配置id: {id}");
#endif
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

    [ProtoContract]
	public partial class MonsterConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>模型Id</summary>
		[ProtoMember(2)]
		public int UnitId { get; set; }
		/// <summary>AI路径</summary>
		[ProtoMember(3)]
		public string AIPath { get; set; }
		/// <summary>PoseFSM</summary>
		[ProtoMember(4)]
		public string PoseFSM { get; set; }

	}
}
