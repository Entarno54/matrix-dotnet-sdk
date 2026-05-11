using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Meowtrix.Sdk.Core.Infrastructure.Dto.ClientVersion;
using Meowtrix.Sdk.Core.Infrastructure.Extensions;

namespace Meowtrix.Sdk.Core.Infrastructure.Services
{
    public class ClientService : BaseApiService
    {
        public ClientService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        protected override string ResourcePath => "_matrix/client/versions";

        public async Task<MatrixServerVersionsResponse> GetMatrixClientVersions(Uri address,
            CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient();

            return await httpClient.GetAsJsonAsync<MatrixServerVersionsResponse>(ResourcePath, cancellationToken);
        }

        public async Task<string> GetPreferredApiVersion(Uri address, CancellationToken cancellationToken)
        {
            var versions = await GetMatrixClientVersions(address, cancellationToken);
            var apiVersion = GetPreferredApiVersion(versions);
            return apiVersion;
        }
        
        /// <summary>
        /// Determines which API prefix to use (e.g., "v3" or "r0")
        /// based on supported versions reported by the server.
        /// </summary>
        /// <param name="payload">The payload returned by /_matrix/client/versions</param>
        /// <returns>
        /// "v3" if any "v1.x" spec is found,
        /// "r0" if no "v1.x" is found but "r0.x" is found,
        /// otherwise null if no recognized version is found.
        /// </returns>
        private static string GetPreferredApiVersion(MatrixServerVersionsResponse payload)
        {
            if (payload?.versions == null || !payload.versions.Any())
                return null;

            // 1. Check for v1.x (this implies the server supports /_matrix/client/v3)
            bool supportsV1 = payload.versions.Any(ver => ver.StartsWith("v1."));
            if (supportsV1)
            {
                return "v3";
            }

            // 2. Otherwise, see if there's any r0.x
            bool supportsR0 = payload.versions.Any(ver => ver.StartsWith("r0."));
            if (supportsR0)
            {
                return "r0";
            }

            // 3. No recognized version found
            return null;
        }
    }
}