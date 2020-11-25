// Copyright 2016-2020, Pulumi Corporation

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Pulumi
{
    public interface IResourcePackage
    {
        string Name { get; }
        ProviderResource ConstructProvider(string name, string type, string urn);
        Resource Construct(string name, string type, string urn);
    }

    internal static class ResourcePackages
    {
        private static ImmutableDictionary<string, IResourcePackage>? _resourcePackages;
        private static readonly object _resourcePackagesLock = new object();

        internal static bool TryGetResourcePackage(string name, string version, [NotNullWhen(true)] out IResourcePackage? package)
        {
            lock (_resourcePackagesLock)
            {
                _resourcePackages ??= DiscoverResourcePackages();
            }
            return _resourcePackages.TryGetValue(name, out package);
        }

        private static ImmutableDictionary<string, IResourcePackage> DiscoverResourcePackages()
        {
            return LoadReferencedAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(typeof(IResourcePackage).IsAssignableFrom)
                .SelectMany(type => type.GetConstructors().Where(c => c.GetParameters().Length == 0))
                .Select(c => c.Invoke(new object[0]))
                .Cast<IResourcePackage>()
                .ToDictionary(p => p.Name)
                .ToImmutableDictionary();
        }
        
        // Assemblies are loaded on demand, so it could be that some assemblies aren't yet loaded to the current
        // app domain at the time of discovery. This method iterates through the list of referenced assemblies
        // recursively.
        // Note: If an assembly is referenced but its types are never used anywhere in the program, that reference
        // will be optimized out and won't appear in the result of the enumeration.
        private static IEnumerable<Assembly> LoadReferencedAssemblies()
        {
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                // This will never happen during an actual Pulumi execution.
                assembliesToCheck.Enqueue(entryAssembly);
            }

            while (assembliesToCheck.Any())
            {
                var assemblyToCheck = assembliesToCheck.Dequeue();
                foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
                {
                    if (reference.FullName.StartsWith("System") || loadedAssemblies.Contains(reference.FullName))
                        continue;

                    var assembly = Assembly.Load(reference);
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    yield return assembly;
                }
            }
        }
    }
}
