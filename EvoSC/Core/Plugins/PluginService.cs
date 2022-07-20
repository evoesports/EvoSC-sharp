﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using EvoSC.Core.Helpers;
using EvoSC.Core.Plugins.Abstractions;
using EvoSC.Core.Plugins.Exceptions;
using EvoSC.Core.Plugins.Info;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace EvoSC.Core.Plugins;

public class PluginService : IPluginService
{
    private readonly PluginsHostConfiguration _options;
    private readonly ILogger<PluginService> _logger;
    private readonly IServiceProvider _mainServices;

    private Dictionary<Guid, IPluginLoadContext> _loadedPlugins;
    private List<ISortedPluginCollection> _pluginCollections;

    public PluginService(IOptions<PluginsHostConfiguration> options, ILogger<PluginService> logger, IServiceProvider mainServices)
    {
        _options = options.Value;
        _logger = logger;
        _loadedPlugins = new();
        _mainServices = mainServices;
        _pluginCollections = new();
    }

    private bool IsInternal(string pluginName)
    {
        foreach (var collection in _pluginCollections)
        {
            foreach (var (name, plugin) in collection.Plugins)
            {
                if (name == pluginName && plugin.IsInternal)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<IPluginLoadContext> InitializePlugin(IPluginMetaInfo pluginMeta)
    {
        // load dependencies first
        var dependencies = new List<IPluginMetaInfo>();
        var dependencyInfos = new List<IPluginLoadContext>();

        foreach (var dependency in pluginMeta.Dependencies)
        {
            if (dependency.ResolvedPath is null && !IsInternal(dependency.Name))
            {
                throw new PluginException($"The dependency '{dependency.Name}' was not found.");
            }

            var dependencyMeta = PluginMetaInfo.FromDirectory(dependency.ResolvedPath);
            var depLoadInfo = await InternalLoad(dependencyMeta);
            dependencyInfos.Add(depLoadInfo);
        }
        
        // load all assemblies
        var loadId = Guid.NewGuid();
        AssemblyLoadContext? loadContext = null;
        Type? pluginClass = null;
        var assemblies = new List<Assembly>();

        // todo: load assemblies from dependencies
        
        // get all assemblies
        if (!pluginMeta.IsInternal)
        {
            loadContext = new AssemblyLoadContext(loadId.ToString(), true); 
            
            foreach (var asmFile in pluginMeta.AssemblyFiles)
            {
                var assembly = loadContext.LoadFromAssemblyPath(asmFile.FullName);
            
                // find main plugin class
                var found = false;
                foreach (var module in assembly.Modules)
                {
                    foreach (var type in module.GetTypes())
                    {
                        if (typeof(IPlugin).IsAssignableFrom(type))
                        {
                            pluginClass = type;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            
                assemblies.Add(assembly);
            }   
        }
        else
        {
            pluginClass = pluginMeta.InternalClass;
        }

        if (pluginClass == null)
        {
            throw new PluginException($"Plugin class not found for '{pluginMeta.Name}'");
        }
        
        // set up load contextd
        var loadInfo = new PluginLoadContext
        {
            MetaInfo = pluginMeta,
            LoadId = loadId
        };
        
        loadInfo.SetAssemblyContext(loadContext);
        loadInfo.SetPluginClass(pluginClass);
        
        // services
        var method = ReflectionUtils.GetStaticMethod(pluginClass, "Setup");
        var pluginServiceCollection = new ServiceCollection();
        pluginServiceCollection.AddSingleton<IPluginMetaInfo>(pluginMeta);

        // call setup method
        method?.Invoke(null, new object?[] {pluginServiceCollection});

        var pluginServices = pluginServiceCollection.BuildServiceProvider();
        var pluginServiceProvider = new PluginServiceProvider(_mainServices);
        pluginServiceProvider.AddProvider(loadId, pluginServices);
        
        // dependency providers
        foreach (var dependency in dependencyInfos)
        {
            pluginServiceProvider.AddProvider(dependency.LoadId, dependency.ServiceProvider);
        }
        
        loadInfo.SetServiceProvider(pluginServiceProvider);

        return loadInfo;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<IPluginLoadContext> InternalLoad(IPluginMetaInfo pluginMeta)
    {
        var loadInfo = await InitializePlugin(pluginMeta);

        // instantiate the plugin
        var instance = (IPlugin)ActivatorUtilities.CreateInstance(loadInfo.ServiceProvider, loadInfo.PluginClass);
        loadInfo.SetInstance(instance);
        
        _loadedPlugins.Add(loadInfo.LoadId, loadInfo);
        
        return loadInfo;
    }

    private IEnumerable<Guid> FindGuidByName(string pluginName)
    {
        var guids = new List<Guid>();

        foreach (var plugin in _loadedPlugins)
        {
            if (plugin.Value.MetaInfo.Name == pluginName)
            {
                guids.Add(plugin.Key);
            }
        }

        return guids;
    }

    private IEnumerable<IPluginLoadContext> FindDependentPlugins(string name)
    {
        var plugins = new List<IPluginLoadContext>();

        foreach (var (loadId, plugin) in _loadedPlugins)
        {
            foreach (var dep in plugin.MetaInfo.Dependencies)
            {
                if (dep.Name == name)
                {
                    plugins.Add(plugin);
                    break;
                }
            }
        }

        return plugins;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task LoadPlugin(string dir)
    {
        /*
         * 1. Load metafile
         * 2. Detect dependencies
         * 3. Load dependencies if they are not already loaded
         * 4. Build the plugin's service provider.
         * 5. Load plugin with it's dependencies
         */
        var pluginDir = Path.GetFullPath(dir);
        
        // check if plugin files exists
        if (!Directory.Exists(pluginDir))
        {
            throw new DirectoryNotFoundException("Plugin directory not found.");
        }

        var metaInfo = PluginMetaInfo.FromDirectory(pluginDir);

        return InternalLoad(metaInfo);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnloadPlugin(Guid loadId, bool throwIfNotUnloaded=false)
    {
        if (!_loadedPlugins.ContainsKey(loadId))
        {
            throw new InvalidOperationException($"The plugin with load id '{loadId}' does not exit.");
        }

        var plugin = _loadedPlugins[loadId];
        
        // unload plugins depending on this one first
        foreach (var dependentPlugin in FindDependentPlugins(plugin.MetaInfo.Name))
        {
            ((PluginServiceProvider)plugin.ServiceProvider).RemoveProvider(dependentPlugin.LoadId);
            UnloadPlugin(dependentPlugin.LoadId);
        }

        _logger.LogInformation("Unloading {Name}", plugin.MetaInfo.Name);
        
        // unload the plugin
        if (!plugin.MetaInfo.IsInternal && !plugin.UnloadAssemblies() && throwIfNotUnloaded)
        {
            throw new InvalidOperationException("Failed to unload.");
        }

        // try to make sure references are gone
        plugin.SetServiceProvider(null);
        plugin.SetInstance(null);
        plugin.SetAssemblyContext(null);
        plugin.SetPluginClass(null);
        
        _loadedPlugins.Remove(loadId);
        
        GC.Collect();
        GC.WaitForPendingFinalizers();

        return Task.CompletedTask;
    }

    public async Task LoadCollection(ISortedPluginCollection collection)
    {
        _pluginCollections.Add(collection);
        
        foreach (var plugin in collection.SortedLoadOrder())
        {
            await InternalLoad(plugin);
        }
    }

    public async Task UnloadAll()
    {
        foreach (var (name, plugin) in _loadedPlugins.Reverse())
        {
            await UnloadPlugin(plugin.LoadId);
        }
    }
}