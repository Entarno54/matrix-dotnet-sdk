using System.Runtime.Serialization;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Create
{
    public enum RoomType
    {
        [EnumMember(Value = "room")] Room,

        [EnumMember(Value = "space")] Space
    }
}