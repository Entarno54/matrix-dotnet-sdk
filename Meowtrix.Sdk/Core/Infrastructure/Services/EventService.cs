using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Meowtrix.Sdk.Core.Domain.RoomEvent;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;
using Meowtrix.Sdk.Core.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace Meowtrix.Sdk.Core.Infrastructure.Services
{
    public class EventService : BaseApiService
    {
        public EventService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            
        }

        public async Task<SyncResponse> SyncAsync(string accessToken,
            CancellationToken cancellationToken,
            ulong? timeout = null, string? nextBatch = null)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);
            
            var uri = new Uri($"{httpClient.BaseAddress}{ResourcePath}/sync");

            if (timeout != null)
                uri = uri.AddParameter("timeout", timeout.ToString());

            if (nextBatch != null)
                uri = uri.AddParameter("since", nextBatch);

            // HttpClient httpClient = CreateHttpClient(accessToken);

            return await httpClient.GetAsJsonAsync<SyncResponse>(uri.ToString(), cancellationToken);
        }

        public async Task<EventResponse> SendMessageAsync(string accessToken,
            string roomId, string transactionId,
            string message, string replyToEventId, CancellationToken cancellationToken)
        {
            const string eventType = "m.room.message";
            var model = new MessageEvent(MessageType.Text, message);
            model.SetReplyTo(replyToEventId);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/send/{eventType}/{transactionId}";

            return await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
        }

        public async Task<EventResponse> SendReactionAsync(string accessToken,
            string roomId, string transactionId,
            string reaction, string replyToEventId, CancellationToken cancellationToken)
        {
            const string eventType = "m.reaction";
            var model = new ReactEventRequest(replyToEventId, reaction);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/send/{eventType}/{transactionId}";

            return await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
        }

        public async Task<EventResponse> SendImageAsync(string accessToken,
            string roomId, string transactionId, string filename,
            string mxcUrl, string replyToEventId, CancellationToken cancellationToken)
        {
            var model = new ImageMessageEventRequest(filename, mxcUrl);
            model.SetReplyTo(replyToEventId);
            
            const string eventType = "m.room.message";

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/send/{eventType}/{transactionId}";

            var response = await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
            return response;
        }    
        
        public async Task<EventResponse> SendFileAsync(string accessToken,
            string roomId, string transactionId, string filename,
            string mxcUrl, CancellationToken cancellationToken)
        {
            var model = new FileMessageEvent(filename, mxcUrl);
            
            const string eventType = "m.room.message";

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/send/{eventType}/{transactionId}";

            var response = await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
            return response;
        }

        public async Task<EventResponse> EditMessageAsync(string accessToken,
            string roomId, string transactionId, string eventId,
            string message, CancellationToken cancellationToken)
        {
            const string eventType = "m.room.message";
            var model = new EditEvent(MessageType.Text, message, eventId);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/send/{eventType}/{transactionId}";

            return await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
        }
        
        public async Task SendTypingSignalAsync(string accessToken,
            string roomId, string userId, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var model = new TypingSignalEvent(true, (uint)timeout.TotalMilliseconds);
            await SendTypingSignalAsync(accessToken, roomId, userId, model, cancellationToken);
        }
        public async Task SendTypingSignalAsync(string accessToken,
            string roomId, string userId, bool isTyping, CancellationToken cancellationToken)
        {
            var model = new TypingSignalEvent(isTyping, 0);
            await SendTypingSignalAsync(accessToken, roomId, userId, model, cancellationToken);
        }
        public async Task SendTypingSignalAsync(string accessToken,
            string roomId, string userId, TypingSignalEvent typingEvent, CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/typing/{userId}";
            await httpClient.PutAsJsonAsync<EventResponse>(path, typingEvent, cancellationToken);
        }
        
        private static readonly string RoomMessagesFilter = HttpUtility.UrlEncode(
            JsonConvert.SerializeObject(
                new Dictionary<string, bool>
                {
                    { "lazy_load_members", true }
                }));

        public struct RoomMessagesResponse
        {
            public struct Unsigned
            {
                public long age;
            }

            public string start;

            public RoomEventResponse[] chunk;
            public RoomEventResponse[] state;
            
            public string end;
        }
        
        private HttpClient historyHttpClient;
        public async Task<List<BaseRoomEvent>> GetTimelineEventsAsync(string accessToken, string roomId, string fromEventId, Func<BaseRoomEvent, Task<bool>> stopCallback, CancellationToken cancellationToken)
        {
            if (historyHttpClient == null)
            {
                var httpClientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
                historyHttpClient = new HttpClient(httpClientHandler);
                
                // this is a singleton and can't be changed, we need to use a different HttpClient
                HttpClient defaultClient = CreateHttpClient(accessToken);
                historyHttpClient.AddBearerToken(accessToken);
                historyHttpClient.BaseAddress = defaultClient.BaseAddress;
                historyHttpClient.MaxResponseContentBufferSize = defaultClient.MaxResponseContentBufferSize;
                historyHttpClient.Timeout = TimeSpan.FromMinutes(5);
            }
            
            var events = new List<BaseRoomEvent>();

            bool hasHitFromEvent = false;
            if (fromEventId == null) hasHitFromEvent = true;
            string fromToken = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                var messagesToPull = 1000;

                var path = $"{ResourcePath}/rooms/{roomId}/messages?limit={messagesToPull}&dir=b&filter={RoomMessagesFilter}";
                if (fromToken != null)
                {
                    path += $"&from={fromToken}";
                }

                HttpClient httpClient = CreateHttpClient(accessToken);
                // httpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await historyHttpClient.GetAsJsonAsync<RoomMessagesResponse>(path, cancellationToken);

                foreach (var roomEvent in response.chunk)
                {
                    if (roomEvent.EventId == fromEventId)
                    {
                        hasHitFromEvent = true;
                    }

                    if (!hasHitFromEvent)
                    {
                        continue;
                    }

                    var ev = BaseRoomEvent.Create(roomEvent.RoomId, roomEvent);
                    if (ev != null)
                    {
                        events.Add(ev);
                    }
                    else
                    {
                        //Omg bro who the fuck puts a write like that into a framework that could used by cli apps
                        //Console.WriteLine($"Unable to concretize event: {JsonConvert.SerializeObject(roomEvent, Formatting.Indented)}");
                    }

                    if (ev != null)
                    {
                        if (await stopCallback.Invoke(ev))
                        {
                            return events;
                        }
                    }
                }

                fromToken = response.end;

                if ((response.chunk == null || response.chunk.Length == 0) && (response.state == null || response.state.Length == 0))
                {
                    break;
                }
            }
            return events;
        }

        public async Task<BaseRoomEvent> GetEvent(string accessToken, string eventId, CancellationToken cancellationToken)
        {
            var url = $"{ResourcePath}/events/{eventId}";
            HttpClient httpClient = CreateHttpClient(accessToken);
            var response = await httpClient.GetAsJsonAsync<RoomEventResponse>(url, cancellationToken);
            return BaseRoomEvent.Create(response.RoomId, response);
        }

        public async Task<string> GetString(string accessToken, string url, CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);
            return await httpClient.GetAsStringAsync(url, cancellationToken);
        }
    }
}