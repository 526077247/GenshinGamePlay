using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class I18NConfigCategory : ProtoObject
    {
        [NinoIgnore]
        private Dictionary<int, I18NConfig> dict = new Dictionary<int, I18NConfig>();
        
        [NinoMember(1)]
        private List<I18NConfig> list = new List<I18NConfig>();

        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                I18NConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public I18NConfig Get(int id)
        {
            this.dict.TryGetValue(id, out I18NConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (I18NConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, I18NConfig> GetAll()
        {
            return this.dict;
        }
        public List<I18NConfig> GetAllList()
        {
            return this.list;
        }
        public I18NConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [NinoSerialize]
	public partial class I18NConfig: ProtoObject
	{
		/// <summary>Id</summary>
		[NinoMember(1)]
		public int Id { get; set; }
#if NOT_UNITY
        /// <summary>索引标识</summary>
		public string Key { get; set; }
#endif
		/// <summary>内容</summary>
		[NinoMember(3)]
		public string Value { get; set; }
    }
}
