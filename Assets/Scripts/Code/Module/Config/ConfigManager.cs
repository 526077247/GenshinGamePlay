using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaoTie
{
    public class ConfigManager:IManager
    {
        public IConfigLoader ConfigLoader { get; set; }

        public Dictionary<Type, object> AllConfig = new Dictionary<Type, object>();

        #region override

        public void Init()
        {
            ConfigLoader = new ConfigLoader();
            Load();
        }

        public void Destroy()
        {
        }

        #endregion
        
        public T LoadOneConfig<T>(string name = "", bool cache = false) where T: ProtoObject
		{
			Type configType = TypeInfo<T>.Type;
			if (string.IsNullOrEmpty(name))
				name = configType.FullName;
			byte[] oneConfigBytes = this.ConfigLoader.GetOneConfigBytes(name);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			if(cache)
				this.AllConfig[configType] = category;

			return category as T;
		}
		public void LoadOneConfig(Type configType)
		{
			byte[] oneConfigBytes = this.ConfigLoader.GetOneConfigBytes(configType.FullName);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			this.AllConfig[configType] = category;
		}
		public void Load()
		{
			this.AllConfig.Clear();
			List<Type> types = AttributeManager.Instance.GetTypes(TypeInfo<ConfigAttribute>.Type);

			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			this.ConfigLoader.GetAllConfigBytes(configBytes);

			foreach (Type type in types)
			{
				try
				{
					this.LoadOneInThread(type, configBytes);
				}
				catch (Exception ex)
				{
					Log.Error("加载配置表出错："+type.Name+"\n"+ex);
				}
			}
		}

		public async ETTask LoadAsync()
		{
			this.AllConfig.Clear();
			List<Type> types = AttributeManager.Instance.GetTypes(TypeInfo<ConfigAttribute>.Type);

			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			this.ConfigLoader.GetAllConfigBytes(configBytes);

			using (ListComponent<Task> listTasks = ListComponent<Task>.Create())
			{
				foreach (Type type in types)
				{
					Task assignment = Task.Run(() => this.LoadOneInThread(type, configBytes));
					listTasks.Add(assignment);
				}

				await Task.WhenAll(listTasks.ToArray());
			}
		}

		private void LoadOneInThread(Type configType, Dictionary<string, byte[]> configBytes)
		{
			byte[] oneConfigBytes = configBytes[configType.Name];

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			lock (this)
			{
				this.AllConfig[configType] = category;
			}
		}
    }
}