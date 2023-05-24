using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class EquipConfigCategory : ProtoObject, IMerge
    {
        public static EquipConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, EquipConfig> dict = new Dictionary<int, EquipConfig>();
        
        [NinoMember(1)]
        private List<EquipConfig> list = new List<EquipConfig>();
		
        public EquipConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            EquipConfigCategory s = o as EquipConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                EquipConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public EquipConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EquipConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EquipConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EquipConfig> GetAll()
        {
            return this.dict;
        }
        public List<EquipConfig> GetAllList()
        {
            return this.list;
        }
        public EquipConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class EquipConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>模型Id</summary>
		[NinoMember(2)]
		public int UnitId { get; set; }
		/// <summary>挂点</summary>
		[NinoMember(3)]
		public string AttachPointType { get; set; }

	}
}
