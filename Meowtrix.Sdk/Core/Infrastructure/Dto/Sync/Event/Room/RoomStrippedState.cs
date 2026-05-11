using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room
{
    public record RoomStrippedState : BaseEvent
    {
        /// <summary>
        ///     <b>Required.</b> Contains the fully-qualified ID of the user who sent this event.
        /// </summary>
        [JsonProperty("sender")]
        public string SenderUserId { get; init; }
    }
}