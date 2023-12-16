using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.EventSourcing.EventStorage;
using Orleans.EventSourcing.EventStorage.Marten;
using Orleans.Runtime;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public static class MartenEventStorageSiloBuilderExtensions
{
    /// <summary>
    /// Configure silo to use Marten as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddMartenEventStorageAsDefault(
        this ISiloBuilder builder,
        Action<MartenOptions> configureOptions
    )
    {
        return builder.AddMartenEventStorageAsDefault(ob => ob.Configure(configureOptions));
    }

    /// <summary>
    /// Configure silo to use Marten as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddMartenEventStorageAsDefault(
        this ISiloBuilder builder,
        Action<OptionsBuilder<MartenOptions>>? configureOptions = null
    )
    {
        return builder.AddMartenEventStorage(
            EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME,
            configureOptions
        );
    }

    /// <summary>
    /// Configure silo to use Marten as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name of the event storage provider. This must match the <c>ProviderName</c> property specified when injecting state into a grain.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddMartenEventStorage(
        this ISiloBuilder builder,
        string name,
        Action<MartenOptions> configureOptions
    )
    {
        return builder.AddMartenEventStorage(
            name,
            ob => ob.Configure(configureOptions)
        );
    }

    /// <summary>
    /// Configure the silo to use Marten for event storage
    /// </summary>
    /// <param name="builder">The silo builder</param>
    /// <param name="name">The name of the event storage provider. This must match the <c>ProviderName</c> property specified when injecting state into a grain.</param>
    /// <param name="configureOptions">The configuration delegate</param>
    /// <returns>The silo builder</returns>
    public static ISiloBuilder AddMartenEventStorage(
        this ISiloBuilder builder,
        string name,
        Action<OptionsBuilder<MartenOptions>>? configureOptions = null
    )
    {
        return builder.ConfigureServices(services =>
        {
            configureOptions?.Invoke(services.AddOptions<MartenOptions>(name));
            services.ConfigureNamedOptionForLogging<MartenOptions>(name);
            services.AddTransient<IConfigurationValidator>(
                sp => new MartenOptionsValidator(
                    sp.GetService<IOptionsMonitor<MartenOptions>>()!.Get(name),
                    name
                )
            );

            const string defaultProviderName = EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME;
            if (string.Equals(name, defaultProviderName, StringComparison.Ordinal))
            {
                services.TryAddSingleton(
                    sp => sp.GetRequiredKeyedService<IEventStorage>(defaultProviderName)
                );
            }

            services.AddKeyedSingleton(name, MartenEventStorageFactory.Create);
            services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(s => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredKeyedService<IEventStorage>(name));
        });
    }
}