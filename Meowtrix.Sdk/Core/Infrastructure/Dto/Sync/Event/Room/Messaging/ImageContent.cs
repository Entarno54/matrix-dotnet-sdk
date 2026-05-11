namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.Messaging
{
    public record ImageContent : BaseMessageContent
    {
        public string url;

        public record Info(int h, int w, int size, string mimetype);

        public Info info;
    }
}