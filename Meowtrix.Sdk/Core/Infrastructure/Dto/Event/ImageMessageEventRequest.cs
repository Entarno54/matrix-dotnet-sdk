using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Event
{
    public record ImageMessageEventRequest(string body, string url)
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType msgtype = MessageType.Image;
        
        public void SetReplyTo(string replyToEventId)
        {
            if (replyToEventId == null)
            {
                relatesTo = null;
                return;
            }
            
            relatesTo = new RelatesTo()
            {
                inReplyTo = new RelatesTo.InReplyTo()
                {
                    event_id = replyToEventId
                }
            };
        }
        public record RelatesTo
        {
            public record InReplyTo
            {
                public string event_id;
            }

            [JsonProperty("m.in_reply_to")]
            public InReplyTo inReplyTo;
        }

        [JsonProperty("m.relates_to")]
        public RelatesTo relatesTo;
    }
}
