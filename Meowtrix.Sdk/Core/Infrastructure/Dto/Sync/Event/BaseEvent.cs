using Newtonsoft.Json.Linq;

namespace Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event
{
    public record BaseEvent
    {
        /// <summary>
        ///     <b>Required.</b> The type of event.
        ///     This SHOULD be namespaced similar to Java package naming conventions e.g. 'com.example.subdomain.event.type'
        /// </summary>
        public JObject Content { get; init; }
        
        /// <summary>
        ///     A unique key which defines the overwriting semantics for this piece of room state.
        ///     This value is often a zero-length string.
        ///     The presence of this key makes this event a State Event.
        /// </summary>
        public string StateKey { get; init; }
        
        public string redacts;

        /// <summary>
        ///     <b>Required.</b> The fields in this object will vary depending on the type of event.
        ///     When interacting with the REST API, this is the HTTP body.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public EventType EventType { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public string Type
        {
            set => EventType = value switch
            {
                Constants.EventType.Create => EventType.Create,
                Constants.EventType.Member => EventType.Member,
                Constants.EventType.Message => EventType.Message,
                Constants.EventType.Redaction => EventType.Redaction,
                Constants.EventType.Reaction => EventType.Reaction,
                _ => EventType.Unknown
            };
        }
    }
}