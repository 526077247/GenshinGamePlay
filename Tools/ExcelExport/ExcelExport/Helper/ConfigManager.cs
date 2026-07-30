using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaoTie
{

    public class ConfigManager
    {
        public static T GetConfig<T>() where T : ProtoObject
        {
            return default;
        }
    }
}
