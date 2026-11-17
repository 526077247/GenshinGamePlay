using System;
using System.Collections.Generic;
using ProtoBuf;

using System.Numerics;
namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class ElementReactionCategory : ProtoObject, IMerge
    {
        public static ElementReactionCategory Instance => ConfigManager.GetConfig<ElementReactionCategory>();

        
        [ProtoIgnore]
        private Dictionary<int, ElementReaction> dict = new Dictionary<int, ElementReaction>();
        
        [ProtoMember(1)]
        private List<ElementReaction> list = new List<ElementReaction>();
		
        public ElementReactionCategory()
        {
        }
        
        public void Merge(object o)
        {
            ElementReactionCategory s = o as ElementReactionCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                ElementReaction config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
                config.AfterEndInit();
            }            
            this.AfterEndInit();
        }
		
        public ElementReaction Get(int id)
        {
            this.dict.TryGetValue(id, out ElementReaction item);

            if (item == null)
            {
#if !NOT_UNITY
                Log.Error($"配置找不到，配置表名: {nameof (ElementReaction)}，配置id: {id}");
#endif
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ElementReaction> GetAll()
        {
            return this.dict;
        }
        public List<ElementReaction> GetAllList()
        {
            return this.list;
        }
        public ElementReaction GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class ElementReaction: ProtoObject
	{
		/// <summary>ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名字（只用于日志与表现，代码只认 Id）</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>反应的一方元素（填元素 Id）</summary>
		[ProtoMember(3)]
		public int ElementA { get; set; }
		/// <summary>反应的另一方元素（填元素 Id；与 ElementA 不分先后，两两组合只认一条）</summary>
		[ProtoMember(4)]
		public int ElementB { get; set; }

	}
}
