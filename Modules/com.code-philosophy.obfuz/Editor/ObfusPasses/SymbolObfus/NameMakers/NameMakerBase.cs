﻿using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obfuz.ObfusPasses.SymbolObfus.NameMakers
{
    public abstract class NameMakerBase : INameMaker
    {

        private readonly Dictionary<object, INameScope> _nameScopes = new Dictionary<object, INameScope>();

        private readonly object _namespaceScope = new object();

        protected abstract INameScope CreateNameScope();

        protected INameScope GetNameScope(object key)
        {
            if (!_nameScopes.TryGetValue(key, out var nameScope))
            {
                nameScope = CreateNameScope();
                _nameScopes[key] = nameScope;
            }
            return nameScope;
        }

        public void AddPreservedName(TypeDef typeDef, string name)
        {
            GetNameScope(typeDef.Module).AddPreservedName(name);
        }

        public void AddPreservedName(MethodDef methodDef, string name)
        {
            GetNameScope(methodDef.DeclaringType).AddPreservedName(name);
        }

        public void AddPreservedName(FieldDef fieldDef, string name)
        {
            GetNameScope(fieldDef.DeclaringType).AddPreservedName(name);
        }

        public void AddPreservedName(PropertyDef propertyDef, string name)
        {
            GetNameScope(propertyDef.DeclaringType).AddPreservedName(name);
        }

        public void AddPreservedName(EventDef eventDef, string name)
        {
            GetNameScope(eventDef.DeclaringType).AddPreservedName(name);
        }

        public void AddPreservedNamespace(TypeDef typeDef, string name)
        {
            GetNameScope(_namespaceScope).AddPreservedName(name);
        }

        private string GetDefaultNewName(object scope, string originName)
        {
            return GetNameScope(scope).GetNewName(originName, false);
        }

        public string GetNewNamespace(TypeDef typeDef, string originalNamespace, bool reuse)
        {
            if (string.IsNullOrEmpty(originalNamespace))
            {
                return string.Empty;
            }
            return GetNameScope(_namespaceScope).GetNewName(originalNamespace, reuse);
        }

        public string GetNewName(TypeDef typeDef, string originalName)
        {
            return GetDefaultNewName(typeDef.Module, originalName);
        }

        public string GetNewName(MethodDef methodDef, string originalName)
        {
            return (methodDef.IsVirtual ? ">" : "") + GetDefaultNewName(methodDef.DeclaringType, originalName);
        }

        public virtual string GetNewName(ParamDef param, string originalName)
        {
            return "1";
        }

        public string GetNewName(FieldDef fieldDef, string originalName)
        {
            return GetDefaultNewName(fieldDef.DeclaringType, originalName);
        }

        public string GetNewName(PropertyDef propertyDef, string originalName)
        {
            return GetDefaultNewName(propertyDef.DeclaringType, originalName);
        }

        public string GetNewName(EventDef eventDef, string originalName)
        {
            return GetDefaultNewName(eventDef.DeclaringType, originalName);
        }
    }
}
