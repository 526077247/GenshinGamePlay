using System;
using System.Collections.Generic;
using ProtoBuf;

using System.Numerics;
namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class SceneConfigCategory : ProtoObject, IMerge
    {
        public static SceneConfigCategory Instance => ConfigManager.GetConfig<SceneConfigCategory>();

        
        [ProtoIgnore]
        private Dictionary<int, SceneConfig> dict = new Dictionary<int, SceneConfig>();
        
        [ProtoMember(1)]
        private List<SceneConfig> list = new List<SceneConfig>();
		
        public SceneConfigCategory()
        {
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
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public SceneConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SceneConfig item);

            if (item == null)
            {
#if !NOT_UNITY
                Log.Error($"配置找不到，配置表名: {nameof (SceneConfig)}，配置id: {id}");
#endif
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

    [ProtoContract]
	public partial class SceneConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名字</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public string Desc { get; set; }
		/// <summary>场景路径</summary>
		[ProtoMember(4)]
		public string Perfab { get; set; }
		/// <summary>是否日夜循环环境类型</summary>
		[ProtoMember(5)]
		public int DayNight { get; set; }
		/// <summary>环境配置参数（日夜循环填4个否则填1个）</summary>
		[ProtoMember(6)]
		public int[] EnvIds { get; set; }
		/// <summary>初始生成SceneGroup</summary>
		[ProtoMember(7)]
		public ulong[] SceneGroupIds { get; set; }

	}
}
