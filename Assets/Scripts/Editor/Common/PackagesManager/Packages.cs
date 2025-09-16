using System.Collections.Generic;
using Newtonsoft.Json;
namespace TaoTie
{
    public class Packages
    {
        public Dictionary<string, string> dependencies;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<Scoped> scopedRegistries;
    }
}