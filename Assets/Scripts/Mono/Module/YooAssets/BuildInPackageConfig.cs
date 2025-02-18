using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class BuildInPackageConfig: ScriptableObject
    {
        public List<string> PackageName;
        public List<int> PackageVer;
    }
}