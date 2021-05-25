using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Hangfire.HttpJob.Agent.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Hangfire.HttpJob.Agent
{
    public static class JobAgentServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireHttpJobAgent(this IServiceCollection serviceCollection, Action<JobAgentServiceConfigurer> configure = null)
        {
            serviceCollection.AddOptions();
            serviceCollection.TryAddSingleton<IConfigureOptions<JobAgentOptions>, ConfigureJobAgentOptions>();
            var configurer = new JobAgentServiceConfigurer(serviceCollection);
            if (configure == null)
            {
                var assembly = Assembly.GetEntryAssembly();
                configure = (c) => { c.AddJobAgent(assembly); };
            }
            configure.Invoke(configurer);
            serviceCollection.TryAddSingleton<JobAgentMiddleware>();

            serviceCollection.AddHotReloadListen(configure);

            return serviceCollection;

        }

        const string DEFAULT_PATH = "dlls";
        public static IServiceCollection AddHotReloadListen(this IServiceCollection serviceCollection, Action<JobAgentServiceConfigurer> configure = null)
        {
            var path = Path.Combine(Environment.CurrentDirectory, DEFAULT_PATH);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var watcher = new FileSystemWatcher(path);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Created += (sender, e) =>
            {
                var configurer = new JobAgentServiceConfigurer(serviceCollection);
                if (configure == null)
                {
                    var assembly = Assembly.LoadFrom(e.FullPath);
                    configure = (c) => { c.AddJobAgent(assembly); };
                }
                configure.Invoke(configurer);
            };

            watcher.Filter = "*.dll";
            watcher.EnableRaisingEvents = true;

            return serviceCollection;
        }

    }


}
