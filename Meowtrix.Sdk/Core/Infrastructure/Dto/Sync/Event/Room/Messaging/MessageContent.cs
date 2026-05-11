using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.Messaging
{
    public enum MessageType
    {
        Text,
        Image,
        Unknown
    }

    public record MessageContent : BaseMessageContent
    {
        public record RelatesTo
        {
            public string rel_type;
            public string event_id;
            public string key;

            public record ReplyTo
            {
                public string event_id;
            }
            
            [JsonProperty("m.in_reply_to")] 
            public ReplyTo in_reply_to;
        }

        [JsonProperty("m.relates_to")] 
        public RelatesTo relatesTo;

        [JsonIgnore]
        public string ReplacesEventId
        {
            get
            {
                if (relatesTo != null && relatesTo.rel_type == "m.replace")
                {
                    return relatesTo.event_id;
                }
                return string.Empty;
            }
        }

        [JsonProperty("m.new_content")] public MessageContent newContent;
        
        [JsonIgnore]
        public string ReplyToEventId
        {
            get
            {
                if (relatesTo != null && relatesTo.in_reply_to != null)
                {
                    return relatesTo.in_reply_to.event_id;
                }
                return string.Empty;
            }
        }
        
        [JsonIgnore]
        public string CleanBody
        {
            get
            {
                if (relatesTo != null && relatesTo.rel_type == "m.replaces")
                {
                    return Body.Substring(3);
                }
                return Body;
            }
        }

        public string url;
    }
}