using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Logicware.Connect.Sdk.DependencyInjection;

/// <summary>
/// Extension methods for wiring <see cref="LogicwareConnectClient"/> into
/// an ASP.NET Core / worker DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string HttpClientName = "Logicware.Connect.Sdk";

    /// <summary>
    /// Register <see cref="LogicwareConnectClient"/> (and all resource
    /// classes) as singletons. The inner <see cref="HttpClient"/> is managed
    /// by <c>IHttpClientFactory</c> so socket lifetimes are correct under load.
    /// </summary>
    public static IServiceCollection AddLogicwareConnect(
        this IServiceCollection services,
        Action<LogicwareConnectOptions> configure)
    {
        services.AddOptions<LogicwareConnectOptions>().Configure(configure);
        services.AddHttpClient(HttpClientName).ConfigureHttpClient((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<LogicwareConnectOptions>>().Value;
            client.Timeout = opts.Timeout;
        });

        services.AddSingleton<LogicwareConnectClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<LogicwareConnectOptions>>().Value;
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var boundOpts = new LogicwareConnectOptions
            {
                ApiKey = opts.ApiKey,
                BaseUrl = opts.BaseUrl,
                Timeout = opts.Timeout,
                MaxAttempts = opts.MaxAttempts,
                UserAgentSuffix = opts.UserAgentSuffix,
                Recorder = opts.Recorder,
                HttpClient = factory.CreateClient(HttpClientName),
            };
            return new LogicwareConnectClient(boundOpts);
        });

        return services;
    }
}
