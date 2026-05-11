using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Meowtrix.Sdk.Core.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Services
{
    public class MediaService : BaseApiService
    {
        public MediaService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }
    
        private struct UploadResponse
        {
            public string content_uri;
        }
        
        public async Task<string> UploadImage(string accessToken, string filename, byte[] imageData, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(filename);
            if (extension != ".png" && extension != ".gif")
            {
                throw new Exception($"only png and gif uploads are supported: {filename}");
            }
            
            string encodedFilename = HttpUtility.UrlEncode(filename);
            string url = $"{MediaPath}/upload?filename={encodedFilename}";
    
            HttpClient httpClient = CreateHttpClient(accessToken);
            using (var content = new ByteArrayContent(imageData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("image/"+extension.Substring(1));
                content.Headers.ContentLength = imageData.Length;
                
                var response = await httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UploadResponse>(json).content_uri;
            }
        }

        public async Task<string> UploadFile(string accessToken, string filename, byte[] imageData, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension))
            {
                throw new Exception($"only uploads with filename and extension are supported: {filename}");
            }

            string encodedFilename = HttpUtility.UrlEncode(filename);
            string url = $"{MediaPath}/upload?filename={encodedFilename}";

            HttpClient httpClient = CreateHttpClient(accessToken);
            using (var content = new ByteArrayContent(imageData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/" + extension.Remove(0, 1));
                content.Headers.ContentLength = imageData.Length;

                var response = await httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UploadResponse>(json).content_uri;
            }
        }

        public async Task<byte[]> GetMedia(string accessToken, string mxcUrl, CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);

            if (string.IsNullOrWhiteSpace(mxcUrl) || !mxcUrl.StartsWith("mxc://", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Invalid mxc url: {mxcUrl}");

            var mxcPath = mxcUrl.Substring("mxc://".Length);
            var sep = mxcPath.IndexOf('/');
            if (sep <= 0 || sep == mxcPath.Length - 1)
                throw new Exception($"Malformed mxc url: {mxcUrl}");

            var mediaServer = mxcPath.Substring(0, sep);
            var mediaId = mxcPath.Substring(sep + 1);

            // Synapse authenticated media uses the client endpoint; try that first.
            var clientPath = $"_matrix/client/v1/media/download/{mediaServer}/{mediaId}?allow_redirect=true";
            try
            {
                return await httpClient.GetAsBytesAsync(clientPath, cancellationToken);
            }
            catch
            {
                // Fallback for older homeservers that still expose /_matrix/media/*.
                var mediaPath = $"{MediaPath}/download/{mediaServer}/{mediaId}?allow_redirect=true";
                return await httpClient.GetAsBytesAsync(mediaPath, cancellationToken);
            }
        }
    }
}
