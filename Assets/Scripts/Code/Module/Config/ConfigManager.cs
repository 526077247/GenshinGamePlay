using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaoTie
{
    public class ConfigManager:IManager
    {
	    public static ConfigManager Instance { get; private set; }
        public IConfigLoader ConfigLoader { get; set; }

        public Dictionary<Type, object> AllConfig = new ();

        #region override

        public void Init()
        {
	        Instance = this;
            ConfigLoader = new ConfigLoader();
        }

        public void Destroy()
        {
	        Instance = null;
	        ConfigLoader = null;
	        AllConfig.Clear();
        }

        #endregion
        
        public async ETTask<T> LoadOneConfig<T>(string name = "", bool cache = false) where T: ProtoObject
		{
			Type configType = TypeInfo<T>.Type;
			if (string.IsNullOrEmpty(name))
				name = configType.FullName;
			byte[] oneConfigBytes = await this.ConfigLoader.GetOneConfigBytes(name);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			if(cache)
				this.AllConfig[configType] = category;

			return category as T;
		}

        public async ETTask LoadAsync()
		{
			this.AllConfig.Clear();
			List<Type> types = AttributeManager.Instance.GetTypes(TypeInfo<ConfigAttribute>.Type);

			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			await this.ConfigLoader.GetAllConfigBytes(configBytes);
#if UNITY_WEBGL
			foreach (Type type in types)
			{
				this.LoadOneInThread(type, configBytes);
			}	
#else
			using (ListComponent<Task> listTasks = ListComponent<Task>.Create())
			{
				foreach (Type type in types)
				{
					Task assignment = Task.Run(() => this.LoadOneInThread(type, configBytes));
					listTasks.Add(assignment);
				}

				await Task.WhenAll(listTasks.ToArray());
			}
#endif
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

		public void ReleaseConfig<T>() where T : ProtoObject, IMerge
		{
			Type configType = TypeInfo<T>.Type;
			AllConfig.Remove(configType);
		}
    }
}