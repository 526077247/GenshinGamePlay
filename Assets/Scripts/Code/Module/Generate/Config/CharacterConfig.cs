using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class CharacterConfigCategory : ProtoObject, IMerge
    {
        public static CharacterConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, CharacterConfig> dict = new Dictionary<int, CharacterConfig>();
        
        [NinoMember(1)]
        private List<CharacterConfig> list = new List<CharacterConfig>();
		
        public CharacterConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            CharacterConfigCategory s = o as CharacterConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                CharacterConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public CharacterConfig Get(int id)
        {
            this.dict.TryGetValue(id, out CharacterConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (CharacterConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, CharacterConfig> GetAll()
        {
            return this.dict;
        }
        public List<CharacterConfig> GetAllList()
        {
            return this.list;
        }
        public CharacterConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class CharacterConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[NinoMember(2)]
		public string Name { get; set; }
		/// <summary>描述</summary>
		[NinoMember(3)]
		public string Desc { get; set; }
		/// <summary>3D模型</summary>
		[NinoMember(4)]
		public int UnitId { get; set; }
		/// <summary>全身立绘</summary>
		[NinoMember(5)]
		public string FullBody { get; set; }
		/// <summary>半身立绘</summary>
		[NinoMember(6)]
		public string HalfBody { get; set; }
		/// <summary>头像</summary>
		[NinoMember(7)]
		public string HeadIcon { get; set; }

	}
}
