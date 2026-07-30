using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaoTie
{
    public class ConfigManager:IManager
    {
	    public static ConfigManager Instance { get; private set; }
        public IConfigLoader ConfigLoader { get; set; }

        public Dictionary<Type, object> AllConfig = new Dictionary<Type, object>();

        private static Dictionary<string, byte[]> cachedBytes;

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
	        cachedBytes?.Clear();
	        cachedBytes = null;
        }

        #endregion

        public static T GetConfig<T>() where T: ProtoObject
        {
	        Type type = TypeInfo<T>.Type;
	        if (Instance.AllConfig.TryGetValue(type, out var obj))
		        return obj as T;

	        if (cachedBytes != null && cachedBytes.TryGetValue(type.Name, out var bytes))
	        {
		        obj = ProtobufHelper.FromBytes(type, bytes, 0, bytes.Length);
		        lock (Instance.AllConfig)
			        Instance.AllConfig[type] = obj;
		        return obj as T;
	        }
	        return null;
        }

        public async ETTask<T> LoadOneConfig<T>(string name = "", bool cache = false) where T: ProtoObject
		{
			Type configType = TypeInfo<T>.Type;
			if (string.IsNullOrEmpty(name))
				name = configType.Name;
			byte[] oneConfigBytes = await this.ConfigLoader.GetOneConfigBytes(name);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			if(cache)
				this.AllConfig[configType] = category;

			return category as T;
		}

        public async ETTask LoadAsync()
		{
			this.AllConfig.Clear();
			cachedBytes = new Dictionary<string, byte[]>();
			await this.ConfigLoader.GetAllConfigBytes(cachedBytes);
		}

        public void ReleaseConfig<T>() where T : ProtoObject, IMerge
		{
			Type configType = TypeInfo<T>.Type;
			AllConfig.Remove(configType);
		}
    }
}