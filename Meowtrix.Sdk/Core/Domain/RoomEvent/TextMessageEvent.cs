using System;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room.Messaging;
using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Domain.RoomEvent
{
    public record TextMessageEvent(string EventId, string RoomId, string SenderUserId, DateTimeOffset Timestamp, string Message, string ReplacesEventId, string ReplyToEventId) : BaseRoomEvent(EventId, RoomId,
        SenderUserId, Timestamp)
    {
        /// <summary>
        /// Populate this elsewhere (perhaps from a database)
        /// </summary>
        [JsonIgnore]
        public BaseRoomEvent ReplyToEvent { get; set; }
        
        [JsonIgnore]
        public BaseRoomEvent ReplacesEvent { get; set; }
        
        public static class Factory
        { 
        // This is the best way to get a string for edited messages with quotes, unfortunately 
        private const string EditMarker = " * ";
        public static string RemoveEditedAsterisk(string msgText, string full_body)
        {
            // Find the position of the edited message within the full body
            int index = full_body.IndexOf(EditMarker + msgText);
            
            // If the edited message is found and the asterisk exists, replace the indicated string
            if (index != -1)
            {
                // Remove the asterisk and the preceding space
                full_body = full_body.Remove(index, EditMarker.Length);
            }
            
            return full_body;
        }
            
            public static bool TryCreateFrom(RoomEventResponse roomEvent, string roomId, out TextMessageEvent textMessageEvent)
            {
                MessageContent content = roomEvent.Content.ToObject<MessageContent>();
                if (roomEvent.EventType == EventType.Message && content?.MessageType == MessageType.Text)
                {
                    var body = content.Body;
                    if (!string.IsNullOrWhiteSpace(content.ReplacesEventId))
                    {
                        if (content.Body.StartsWith("> <@") && content.Body.Contains("\n\n * "))
                        {
                            // we have a reply, need to clean this up so that it matches unedited messages
                            body = RemoveEditedAsterisk(content.newContent.Body, content.Body);
                        }
                        else
                        {
                            body = content.newContent.Body;
                        }
                    }
                    textMessageEvent = new TextMessageEvent(roomEvent.EventId, roomId, roomEvent.SenderUserId, roomEvent.Timestamp, body, content.ReplacesEventId, content.ReplyToEventId);
                    return true;
                }

                textMessageEvent = null;
                return false;
            }

            public static bool TryCreateFromStrippedState(RoomStrippedState roomStrippedState, string roomId,
                out TextMessageEvent textMessageEvent)
            {
                MessageContent content = roomStrippedState.Content.ToObject<MessageContent>();
                if (roomStrippedState.EventType == EventType.Message && content?.MessageType == MessageType.Text)
                {
                    textMessageEvent = new TextMessageEvent(string.Empty, roomId, roomStrippedState.SenderUserId, DateTimeOffset.MinValue, content.Body, content.ReplacesEventId, content.ReplyToEventId);
                    return true;
                }

                textMessageEvent = null;
                return false;
            }
        }
    }
}