using System.Collections.Generic;

namespace TaoTie
{
    public static class NumericTypeHelper
    {
#if UNITY_EDITOR
        public static (string, int)[] ToArray()
        {
            List<(string, int)> nodeList = new List<(string, int)>();
            var fields = typeof(NumericType).GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic)
                {
                    nodeList.Add((field.Name, (int) field.GetValue(null)));
                }
            }

            return nodeList.ToArray();
        }
#endif
    }
}