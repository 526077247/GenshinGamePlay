using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class ServerConfigCategory : ProtoObject, IMerge
    {
        public static ServerConfigCategory Instance;
		
        [ProtoIgnore]
        private Dictionary<int, ServerConfig> dict = new Dictionary<int, ServerConfig>();
        
        [ProtoMember(1)]
        private List<ServerConfig> list = new List<ServerConfig>();
		
        public ServerConfigCategory()
        {
            Instance = this;
        }
		
        public void Merge(object o)
        {
            ServerConfigCategory s = o as ServerConfigCategory;
            this.list.AddRange(s.list);
        }

        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                ServerConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public ServerConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ServerConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ServerConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ServerConfig> GetAll()
        {
            return this.dict;
        }
        public List<ServerConfig> GetAllList()
        {
            return this.list;
        }
        public ServerConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class ServerConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>标记</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>realm服地址</summary>
		[ProtoMember(3)]
		public string RealmIp { get; set; }
		/// <summary>更新列表cdn地址</summary>
		[ProtoMember(4)]
		public string UpdateListUrl { get; set; }
		/// <summary>路由cdn地址</summary>
		[ProtoMember(5)]
		public string RouterListUrl { get; set; }
		/// <summary>热更资源cdn地址</summary>
		[ProtoMember(6)]
		public string ResUrl { get; set; }
		/// <summary>测试更新列表cdn地址</summary>
		[ProtoMember(7)]
		public string TestUpdateListUrl { get; set; }
		/// <summary>服务器类型</summary>
		[ProtoMember(8)]
		public int EnvId { get; set; }
		/// <summary>是否默认值</summary>
		[ProtoMember(9)]
		public int IsPriority { get; set; }

	}
}
