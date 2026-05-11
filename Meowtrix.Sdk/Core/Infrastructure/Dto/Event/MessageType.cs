using System.Runtime.Serialization;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Event
{
    public enum MessageType
    {
        [EnumMember(Value = "m.text")] Text,
        [EnumMember(Value = "m.image")] Image,
        [EnumMember(Value = "m.file")] File
    }
}