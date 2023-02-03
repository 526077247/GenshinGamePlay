using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaoTie
{
    public static class ExportHelper
    {
        public static List<string> FindFile(string dirPath) //参数dirPath为指定的目录
        {
            List<string> res = new List<string>();
            //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
            DirectoryInfo Dir = new DirectoryInfo(dirPath);
            try
            {
                foreach (DirectoryInfo d in Dir.GetDirectories()) //查找子目录
                {
                    res.AddRange(FindFile(dirPath + "/" + d.Name));
                }
                foreach (var f in Directory.GetFiles(dirPath)) //查找文件
                {
                    res.Add(f);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return res;
        }
    }
}
