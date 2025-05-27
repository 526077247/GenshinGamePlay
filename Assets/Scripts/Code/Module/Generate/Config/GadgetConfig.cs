using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    [Obfuz.ObfuzIgnore]
    public partial class GadgetConfigCategory : ProtoObject, IMerge
    {
        public static GadgetConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, GadgetConfig> dict = new Dictionary<int, GadgetConfig>();
        
        [NinoMember(1)]
        private List<GadgetConfig> list = new List<GadgetConfig>();
		
        public GadgetConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            GadgetConfigCategory s = o as GadgetConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                GadgetConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public GadgetConfig Get(int id)
        {
            this.dict.TryGetValue(id, out GadgetConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (GadgetConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, GadgetConfig> GetAll()
        {
            return this.dict;
        }
        public List<GadgetConfig> GetAllList()
        {
            return this.list;
        }
        public GadgetConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class GadgetConfig: ProtoObject
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
		/// <summary>PoseFSM</summary>
		[NinoMember(4)]
		public string PoseFSM { get; set; }

	}
}
