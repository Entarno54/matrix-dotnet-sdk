using System.Collections.Generic;
using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Joined
{
    public record JoinedRoomsResponse(List<string> JoinedRoomIds)
    {
        /// <summary>
        ///     <b>Required.</b> The ID of each room in which the user has joined membership.
        /// </summary>
        [JsonProperty("joined_rooms")]
        public List<string> JoinedRoomIds { get; } = JoinedRoomIds;
    }
}