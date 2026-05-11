using System.Net.Http;
using Meowtrix.Sdk.Core.Domain.Services;
using Meowtrix.Sdk.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meowtrix.Sdk
{
    /// <summary>
    ///     Extensions methods to configure an <see cref="IServiceCollection" /> for <see cref="IHttpClientFactory" /> with
    ///     Matrix Sdk.
    /// </summary>
    public static class MatrixClientServiceExtensions
    {
        public static IServiceCollection AddMatrixClient(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory, SingletonHttpFactory>();

            services.AddSingleton<ClientService>();
            services.AddSingleton<EventService>();
            services.AddSingleton<RoomService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<MediaService>(); 
            services.AddTransient<IPollingService, PollingService>();
            services.AddTransient<IMatrixClient, MatrixClient>();

            return services;
        }
    }
}