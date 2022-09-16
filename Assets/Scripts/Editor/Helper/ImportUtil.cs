using System.Reflection;
using UnityEditor;

namespace TaoTie
{


    public class ImportUtil
    {
        //使用反射获取图片的宽和高
        public static void GetTextureRealWidthAndHeight(TextureImporter texImpoter, ref int width, ref int height)
        {
            System.Type type = typeof(TextureImporter);
            System.Reflection.MethodInfo method =
                type.GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            var args = new object[] {width, height};
            method.Invoke(texImpoter, args);
            width = (int) args[0];
            height = (int) args[1];
        }

        // 判断宽和高是否是2的次幂
        public static bool WidthAndHeightIsPowerOfTwo(int width, int height)
        {
            if (IsPowerOfTwo(width) && IsPowerOfTwo(height))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //用二进制与运算，如果一个数是2的次幂，n&(n-1) = 0
        public static bool IsPowerOfTwo(int number)
        {
            if (number <= 0)
            {
                return false;
            }

            return (number & (number - 1)) == 0;
        }
    }
}