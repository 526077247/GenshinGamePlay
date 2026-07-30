using System;
using System.Collections.Generic;
using ProtoBuf;

using System.Numerics;
namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class GadgetConfigCategory : ProtoObject, IMerge
    {
        public static GadgetConfigCategory Instance => ConfigManager.GetConfig<GadgetConfigCategory>();

        
        [ProtoIgnore]
        private Dictionary<int, GadgetConfig> dict = new Dictionary<int, GadgetConfig>();
        
        [ProtoMember(1)]
        private List<GadgetConfig> list = new List<GadgetConfig>();
		
        public GadgetConfigCategory()
        {
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
#if !NOT_UNITY
                Log.Error($"配置找不到，配置表名: {nameof (GadgetConfig)}，配置id: {id}");
#endif
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

    [ProtoContract]
	public partial class GadgetConfig: ProtoObject
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
