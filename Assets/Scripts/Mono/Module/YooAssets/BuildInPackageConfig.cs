using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class BuildInPackageConfig: ScriptableObject
    {
        public List<string> PackageName;
        public List<int> PackageVer;

        public int GetBuildInPackageVersion(string name)
        {
            for (int i = 0; i < PackageName.Count; i++)
            {
                if (PackageName[i] == name)
                {
                    return PackageVer[i];
                }
            }

            return -1;
        }
    }
}