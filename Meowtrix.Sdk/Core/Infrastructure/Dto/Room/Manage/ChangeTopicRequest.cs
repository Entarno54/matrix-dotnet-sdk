namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Manage
{
        public record ChangeTopicRequest(string? topic )
        {
            public string? topic { get; } = topic;
        }

        public record ChangeNameRequest(string? name)
        {
            public string? name { get; } = name;
        }
        
        public record ChangeAvatarRequest(string? url)
        {
            public string? url { get; } = url;
        }
        public record ChangeRoomNickRequest(string? displayname)
        {
            public string? displayname { get; } = displayname;
            public string? membership { get; } = "join";
        }
}