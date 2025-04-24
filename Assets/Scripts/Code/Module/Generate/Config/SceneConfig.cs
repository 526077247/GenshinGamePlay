using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    [Config]
    public partial class SceneConfigCategory : ProtoObject, IMerge
    {
        public static SceneConfigCategory Instance;
		
        
        [NinoIgnore]
        private Dictionary<int, SceneConfig> dict = new Dictionary<int, SceneConfig>();
        
        [NinoMember(1)]
        private List<SceneConfig> list = new List<SceneConfig>();
		
        public SceneConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            SceneConfigCategory s = o as SceneConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                SceneConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public SceneConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SceneConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SceneConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SceneConfig> GetAll()
        {
            return this.dict;
        }
        public List<SceneConfig> GetAllList()
        {
            return this.list;
        }
        public SceneConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class SceneConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
		/// <summary>名字</summary>
		[NinoMember(2)]
		public string Name { get; set; }
		/// <summary>描述</summary>
		[NinoMember(3)]
		public string Desc { get; set; }
		/// <summary>场景路径</summary>
		[NinoMember(4)]
		public string Perfab { get; set; }
		/// <summary>是否日夜循环环境类型</summary>
		[NinoMember(5)]
		public int DayNight { get; set; }
		/// <summary>环境配置参数（日夜循环填4个否则填1个）</summary>
		[NinoMember(6)]
		public int[] EnvIds { get; set; }
		/// <summary>初始生成SceneGroup</summary>
		[NinoMember(7)]
		public ulong[] SceneGroupIds { get; set; }

	}
}
