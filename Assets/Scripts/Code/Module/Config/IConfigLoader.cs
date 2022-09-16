using System.Collections.Generic;

namespace TaoTie
{
    public interface IConfigLoader
    {
        void GetAllConfigBytes(Dictionary<string, byte[]> output);
        byte[] GetOneConfigBytes(string configName);
    }
}