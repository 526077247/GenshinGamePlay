/*
 * Author: Misaka Mikoto
 * email: easy66@live.com
 * github: https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload
 */
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ScriptHotReload
{
    public class HotReloadAssemblyResolver : IAssemblyResolver
    {
        public string baseDir { get; private set; }
        public Dictionary<string, string> fallbackPathes { get; private set; }

        private bool disposedValue;

        public HotReloadAssemblyResolver(string baseDir, Dictionary<string, string> fallbackPathes)
        {
            this.baseDir = baseDir;
            this.fallbackPathes = fallbackPathes;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            string path = GetAssemblyPath(name.Name);
            var assm = AssemblyDefinition.ReadAssembly(path);
            return assm;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            string path = GetAssemblyPath(name.Name);
            var assm = AssemblyDefinition.ReadAssembly(path, parameters);
            return assm;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HotReloadAssemblyResolver()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private string GetAssemblyPath(string name)
        {
            string path = $"{baseDir}/{name}.dll";
            if (!File.Exists(path))
            {
                if (fallbackPathes.TryGetValue(name, out string fallbackPath))
                {
                    path = fallbackPath;
                }
                else
                    throw new Exception($"can not find assembly with name `{name}`");
            }
            return path;
        }
    }

}
