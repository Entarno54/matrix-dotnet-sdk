namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Join
{
    public record GetRoomIdResponse(string RoomId, string[] Servers)
    {
        public string RoomId { get; } = RoomId;
        public string[] Servers { get; } = Servers;
    }
}