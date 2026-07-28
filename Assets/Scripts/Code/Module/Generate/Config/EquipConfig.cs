using System;
using System.Collections.Generic;
using ProtoBuf;

using System.Numerics;
namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class EquipConfigCategory : ProtoObject, IMerge
    {
        public static EquipConfigCategory Instance;
		
        
        [ProtoIgnore]
        private Dictionary<int, EquipConfig> dict = new Dictionary<int, EquipConfig>();
        
        [ProtoMember(1)]
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
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public EquipConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EquipConfig item);

            if (item == null)
            {
#if !NOT_UNITY
                Log.Error($"配置找不到，配置表名: {nameof (EquipConfig)}，配置id: {id}");
#endif
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

    [ProtoContract]
	public partial class EquipConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>模型Id</summary>
		[ProtoMember(2)]
		public int UnitId { get; set; }
		/// <summary>挂点</summary>
		[ProtoMember(3)]
		public string EquipType { get; set; }

	}
}
