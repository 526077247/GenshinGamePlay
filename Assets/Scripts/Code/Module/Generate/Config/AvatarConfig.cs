using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class AvatarConfigCategory : ProtoObject, IMerge
    {
        public static AvatarConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, AvatarConfig> dict = new Dictionary<int, AvatarConfig>();
        
        [NinoMember(1)]
        private List<AvatarConfig> list = new List<AvatarConfig>();
		
        public AvatarConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            AvatarConfigCategory s = o as AvatarConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                AvatarConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public AvatarConfig Get(int id)
        {
            this.dict.TryGetValue(id, out AvatarConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (AvatarConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, AvatarConfig> GetAll()
        {
            return this.dict;
        }
        public List<AvatarConfig> GetAllList()
        {
            return this.list;
        }
        public AvatarConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class AvatarConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>模型Id</summary>
		[NinoMember(2)]
		public int UnitId { get; set; }

	}
}
