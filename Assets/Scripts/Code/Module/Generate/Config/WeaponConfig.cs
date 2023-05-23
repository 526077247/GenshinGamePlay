using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class WeaponConfigCategory : ProtoObject, IMerge
    {
        public static WeaponConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, WeaponConfig> dict = new Dictionary<int, WeaponConfig>();
        
        [NinoMember(1)]
        private List<WeaponConfig> list = new List<WeaponConfig>();
		
        public WeaponConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            WeaponConfigCategory s = o as WeaponConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                WeaponConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public WeaponConfig Get(int id)
        {
            this.dict.TryGetValue(id, out WeaponConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (WeaponConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, WeaponConfig> GetAll()
        {
            return this.dict;
        }
        public List<WeaponConfig> GetAllList()
        {
            return this.list;
        }
        public WeaponConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class WeaponConfig: ProtoObject
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
		/// <summary>FSM路径</summary>
		[NinoMember(6)]
		public string FSM { get; set; }
		/// <summary>Controller路径</summary>
		[NinoMember(7)]
		public string Controller { get; set; }

	}
}
