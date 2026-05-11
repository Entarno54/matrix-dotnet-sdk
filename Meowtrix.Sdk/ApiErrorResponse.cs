using Newtonsoft.Json;

namespace Meowtrix.Sdk
{
    public class ApiErrorResponse
    {
        [JsonProperty("retry_after_ms")]
        public int retryAfterMs;
    }
}

