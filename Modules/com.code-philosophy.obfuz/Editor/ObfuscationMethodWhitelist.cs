﻿using dnlib.DotNet;
using Obfuz.Editor;
using Obfuz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuz
{
    public class ObfuscationMethodWhitelist
    {
        private bool ShouldBeIgnoredByCustomAttribute(IHasCustomAttribute obj)
        {
            return MetaUtil.HasObfuzIgnoreAttribute(obj);
        }

        public bool IsInWhiteList(ModuleDef module)
        {
            string modName = module.Assembly.Name;
            if (modName == "Obfuz.Runtime")
            {
                return true;
            }
            if (ShouldBeIgnoredByCustomAttribute(module))
            {
                return true;
            }
            return false;
        }

        public bool IsInWhiteList(MethodDef method)
        {
            if (IsInWhiteList(method.DeclaringType))
            {
                return true;
            }
            if (method.Name.StartsWith(ConstValues.ObfuzInternalSymbolNamePrefix))
            {
                return true;
            }
            if (ShouldBeIgnoredByCustomAttribute(method))
            {
                return true;
            }
            return false;
        }

        public bool IsInWhiteList(TypeDef type)
        {
            if (type.Name.StartsWith(ConstValues.ObfuzInternalSymbolNamePrefix))
            {
                return true;
            }
            if (IsInWhiteList(type.Module))
            {
                return true;
            }
            if (ShouldBeIgnoredByCustomAttribute(type))
            {
                return true;
            }
            if (type.DeclaringType != null && IsInWhiteList(type.DeclaringType))
            {
                return true;
            }
            if (type.FullName == "Obfuz.EncryptionVM.GeneratedEncryptionVirtualMachine")
            {
                return true;
            }
            return false;
        }
    }
}
