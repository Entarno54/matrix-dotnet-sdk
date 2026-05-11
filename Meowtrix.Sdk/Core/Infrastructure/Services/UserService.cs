// ReSharper disable ArgumentsStyleNamedExpression

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Login;
using Meowtrix.Sdk.Core.Infrastructure.Dto.User;
using Meowtrix.Sdk.Core.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Meowtrix.Sdk.Core.Infrastructure.Services
{
    public class UserService : BaseApiService
    {
        private ILogger logger;
        public UserService(IHttpClientFactory httpClientFactory, ILogger logger=null) : base(httpClientFactory)
        {
            if (logger == null)
            {
                logger = new LoggerFactory().CreateLogger<UserService>();
            }
            
            this.logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(string user, string password, string deviceId,
            CancellationToken cancellationToken)
        {
            logger?.LogInformation($"LoginAsync({user}, {password}, {deviceId})");

            var model = new LoginRequest
            (
                new Identifier
                (
                    "m.id.user",
                    user
                ),
                password,
                deviceId,
                "m.login.password"
            );

            HttpClient httpClient = CreateHttpClient();

            var path = $"{ResourcePath}/login";

            int retries = 10;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    var response = await httpClient.PostAsJsonAsync<LoginResponse>(path, model, cancellationToken);
                    return response;
                }
                catch (ApiException e)
                {
                    logger?.LogWarning(e.ToString());
                    if (e.StatusCode == (HttpStatusCode)429)
                    {
                        var err = JsonConvert.DeserializeObject<ApiErrorResponse>(e.ResponseContent);
                        logger?.LogWarning($"({i+1}/{retries}) Too many requests, waiting {err.retryAfterMs}ms before retrying");
                        await Task.Delay(err.retryAfterMs + 10000);
                    }
                }
            }
            throw new Exception($"Failed to login {retries} times");
        }

        public async Task<MatrixProfile> GetProfile(string accessToken, string fullUserId, CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);
            var path = $"{ResourcePath}/profile/{HttpUtility.HtmlEncode(fullUserId)}";
            return await httpClient.GetAsJsonAsync<MatrixProfile>(path, cancellationToken);
        }


        public class SetDisplaynamePayload
        {
            public string displayname;
        }
        public async Task SetNickname(string accessToken, string fullUserId, string newNick, CancellationToken ctsToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);
            var path = $"{ResourcePath}/profile/{HttpUtility.HtmlEncode(fullUserId)}/displayname";
            await httpClient.PutAsJsonAsync<object>(path, new SetDisplaynamePayload()
            {
                displayname = newNick
            }, ctsToken);
        }
    }
}