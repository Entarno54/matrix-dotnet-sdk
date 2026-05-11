using Markdig;
using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Event
{
    public record MessageEvent(MessageType MessageType, string Message)
    {
        public MessageType msgtype { get; } = MessageType;
        public string body { get; } = Message;
        public string formatted_body { get; } = GetFormattedBody(Message);
        public string format = "org.matrix.custom.html";
        
        private static MarkdownPipeline pipeline = new MarkdownPipelineBuilder().DisableHtml().Build();
        public static string GetFormattedBody(string msg)
        {
            if (msg == null) return null;
            
            // markdown --> html
            var html = Markdown.ToHtml(msg, pipeline);
            
            // strip newline from end
            html = html.TrimEnd('\n');
            
            // strip <p> tags from html
            if (html.StartsWith("<p>") && html.EndsWith("</p>"))
            {
                html = html.Substring(3, html.Length - 7); // remove the starting <p> and ending </p>
            }
            return html;
        }

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