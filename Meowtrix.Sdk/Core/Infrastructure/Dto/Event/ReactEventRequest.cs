using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Event
{
    internal class ReactEventRequest
    {
        public record RelatesTo
        {
            public string key;
            public string event_id;
            public string rel_type = "m.annotation";
        }

        [JsonProperty("m.relates_to")]
        public RelatesTo relatesTo;

        public ReactEventRequest(string event_id, string emoji)
        {
            relatesTo = new RelatesTo()
            {
                event_id = event_id,
                key = emoji
            };
        }
    }
}
