using System;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.Messaging;

namespace Meowtrix.Sdk.Core.Domain.RoomEvent
{
    public record ReactionEvent(string EventId, string RoomId, string SenderUserId, DateTimeOffset Timestamp, string Reaction, string RelatesToEventId) : BaseRoomEvent(EventId, RoomId, SenderUserId, Timestamp)
    {

        public static class Factory
        {
            public static bool TryCreateFrom(RoomEventResponse roomEvent, string roomId, out ReactionEvent reactionEvent)
            {
                // UI.WriteLine(JsonConvert.SerializeObject(roomEvent, new JsonSerializerSettings()
                // {
                //     NullValueHandling = NullValueHandling.Ignore,
                //     Formatting = Formatting.Indented
                // }));
                
                MessageContent content = roomEvent.Content.ToObject<MessageContent>();
                if (roomEvent.EventType == EventType.Reaction && content?.relatesTo?.rel_type == "m.annotation")
                {
                    reactionEvent = new ReactionEvent(roomEvent.EventId, roomId, roomEvent.SenderUserId, roomEvent.Timestamp, content.relatesTo.key, content.relatesTo.event_id);
                    return true;
                }

                reactionEvent = null;
                return false;
            }

            public static bool TryCreateFromStrippedState(RoomStrippedState roomStrippedState, string roomId,
                out ReactionEvent reactionEvent)
            {
                MessageContent content = roomStrippedState.Content.ToObject<MessageContent>();
                if (roomStrippedState.EventType == EventType.Reaction && content?.relatesTo.rel_type == "m.annotation")
                {
                    reactionEvent = new ReactionEvent(string.Empty, roomId, roomStrippedState.SenderUserId, DateTimeOffset.MinValue, content.relatesTo.key, content.relatesTo.event_id);
                    return true;
                }

                reactionEvent = null;
                return false;
            }
        }
    }
}