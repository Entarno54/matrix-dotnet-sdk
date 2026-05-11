using System;
using System.Net.Http;
using Meowtrix.Sdk.Core.Infrastructure.Extensions;

namespace Meowtrix.Sdk.Core.Infrastructure.Services
{
    public abstract class BaseApiService
    {
        // see: https://github.com/dotnet/aspnetcore/issues/28385#issuecomment-853766480
        private readonly IHttpClientFactory _httpClientFactory;

        public Uri? BaseAddress { get; set; }
        private static TimeSpan DefaultTimeout => TimeSpan.FromMilliseconds(Constants.LaterSyncTimout + 10000);

        protected BaseApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string ApiVersion = "v3";
        protected virtual string ResourcePath => $"_matrix/client/{ApiVersion}";
        protected virtual string MediaPath => $"_matrix/media/{ApiVersion}";


        /// <summary>
        ///     Creates HttpClient
        /// </summary>
        /// <param name="accessToken">User access token.</param>
        /// <returns>HttpClient</returns>
        protected HttpClient CreateHttpClient(string? accessToken = null)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.Matrix);

            if (accessToken != null)
                httpClient.AddBearerToken(accessToken);

            if (BaseAddress == null)
                throw new NullReferenceException("set base address");

            if (httpClient.BaseAddress == null)
                httpClient.BaseAddress = BaseAddress;
            
            if (httpClient.Timeout != DefaultTimeout)
                httpClient.Timeout = DefaultTimeout;

            return httpClient;
        }
    }
}