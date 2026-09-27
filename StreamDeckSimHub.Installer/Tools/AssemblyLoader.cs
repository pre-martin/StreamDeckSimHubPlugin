// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System;
using System.IO;
using System.Reflection;

namespace StreamDeckSimHub.Installer.Tools
{
    /// <summary>
    /// Loads NuGet dependencies that are embedded as resources into this assembly (see
    /// "EmbedRuntimeDependencies" target in the .csproj file).
    /// </summary>
    /// <remarks>
    /// This is a lightweight, dependency-free replacement for Costura.Fody: instead of merging the
    /// dependency assemblies into this executable at build time via IL weaving, the dependency DLLs are
    /// simply embedded as resources and loaded on demand via <see cref="AppDomain.AssemblyResolve"/>.
    /// This keeps the "single EXE" deployment goal without relying on a third-party build tool.
    /// </remarks>
    public static class AssemblyLoader
    {
        private static bool _registered;

        /// <summary>
        /// Registers the <see cref="AppDomain.AssemblyResolve"/> handler. Must be called before any code
        /// touches a type from one of the embedded dependencies.
        /// </summary>
        public static void Register()
        {
            if (_registered) return;
            _registered = true;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
        }

        private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
        {
            // args.Name is a full assembly name, e.g. "NLog, Version=5.3.3.0, Culture=neutral, PublicKeyToken=...".
            var assemblyName = new AssemblyName(args.Name).Name;
            var resourceName = assemblyName + ".dll";

            var executingAssembly = Assembly.GetExecutingAssembly();
            using (var stream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return Assembly.Load(memoryStream.ToArray());
                }
            }
        }
    }
}
