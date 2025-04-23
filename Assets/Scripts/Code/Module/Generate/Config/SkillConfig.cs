using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class SkillConfigCategory : ProtoObject, IMerge
    {
        public static SkillConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, SkillConfig> dict = new Dictionary<int, SkillConfig>();
        
        [NinoMember(1)]
        private List<SkillConfig> list = new List<SkillConfig>();
		
        public SkillConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            SkillConfigCategory s = o as SkillConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                SkillConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public SkillConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SkillConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SkillConfig> GetAll()
        {
            return this.dict;
        }
        public List<SkillConfig> GetAllList()
        {
            return this.list;
        }
        public SkillConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class SkillConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>技能名</summary>
		[NinoMember(2)]
		public string Name { get; set; }
		/// <summary>ability名</summary>
		[NinoMember(3)]
		public string AbilityName { get; set; }
		/// <summary>简介</summary>
		[NinoMember(4)]
		public string Desc { get; set; }
		/// <summary>图标</summary>
		[NinoMember(5)]
		public string Icon { get; set; }
		/// <summary>冷却时间公式</summary>
		[NinoMember(6)]
		public string CD { get; set; }

	}
}
