using System;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.State;

namespace Meowtrix.Sdk.Core.Domain.RoomEvent
{
    public record CreateRoomEvent(string EventId, string RoomId, string SenderUserId, string RoomCreatorUserId, DateTimeOffset Timestamp) : BaseRoomEvent(EventId, RoomId,
        SenderUserId, Timestamp)
    {
        public static class Factory
        {
            public static bool TryCreateFrom(RoomEventResponse roomEvent, string roomId, out CreateRoomEvent createRoomEvent)
            {
                RoomCreateContent content = roomEvent.Content.ToObject<RoomCreateContent>();
                if (roomEvent.EventType == EventType.Create && content != null)
                {
                    createRoomEvent = new CreateRoomEvent(roomEvent.EventId, roomId, roomEvent.SenderUserId, content.RoomCreatorUserId, roomEvent.Timestamp);
                    return true;
                }

                createRoomEvent = null;
                return false;
            }

            public static bool TryCreateFromStrippedState(RoomStrippedState roomStrippedState, string roomId,
                out CreateRoomEvent createRoomEvent)
            {
                RoomCreateContent content = roomStrippedState.Content.ToObject<RoomCreateContent>();
                if (roomStrippedState.EventType == EventType.Create && content != null)
                {
                    createRoomEvent =
                        new CreateRoomEvent(string.Empty, roomId, roomStrippedState.SenderUserId, content.RoomCreatorUserId, DateTimeOffset.MinValue);
                    return true;

                }

                createRoomEvent = null;
                return false;
            }
        }
    }
}