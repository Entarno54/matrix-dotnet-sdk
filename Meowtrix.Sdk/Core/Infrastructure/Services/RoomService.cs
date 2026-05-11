using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Create;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Join;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Joined;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Manage;
using Meowtrix.Sdk.Core.Infrastructure.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Meowtrix.Sdk.Core.Infrastructure.Services
{
    public class RoomService : BaseApiService
    {
        public RoomService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<CreateRoomResponse> CreateRoomAsync(string accessToken, string[]? members,
            CancellationToken cancellationToken)
        {
            var model = new CreateRoomRequest
            (
                Invite: members,
                Preset: Preset.TrustedPrivateChat,
                IsDirect: true
            );

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/createRoom";

            return await httpClient.PostAsJsonAsync<CreateRoomResponse>(path, model, cancellationToken);
        }

        public enum InviteResult
        {
            NotInRoom,
            NoPermission,
            AlreadyJoined,
            Banned,
            Success,
        }
        public async Task<InviteResult> InviteToRoomAsync(string accessToken, string senderId, string roomId, string userId, CancellationToken cancellationToken)
        { 
            var path = $"{ResourcePath}/rooms/{roomId}/invite";
            HttpClient httpClient = CreateHttpClient(accessToken);
            try
            {
                await httpClient.PostAsJsonAsync<Dictionary<string, object>>(path, new Dictionary<string, string>
                {
                    { "user_id", userId }
                }, cancellationToken);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.Forbidden)
                {
                    var jObject = JObject.Parse(e.ResponseContent);
                    var errMsg = jObject["error"].ToString();

                    var strippedMsg =
                        errMsg
                            .Replace(senderId, "")
                            .Replace(userId, "")
                            .Replace(roomId, "")
                            .Replace(".", "")
                            .Trim();
                    switch (strippedMsg)
                    {
                        case "You don't have permission to invite users":
                            return InviteResult.NoPermission;
                        case "is already in the room":
                            return InviteResult.AlreadyJoined;
                        case "not in room":
                            return InviteResult.AlreadyJoined;
                        default:
                            throw new NotImplementedException($"Unhandled invite response: {strippedMsg}");
                    }
                }
                
                throw;
            }
            return InviteResult.Success;
        }
        
        public async Task<JoinRoomResponse> JoinRoomAsync(string accessToken, string roomId,
            CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/join/{roomId}";

            return await httpClient.PostAsJsonAsync<JoinRoomResponse>(path, null, cancellationToken);
        }

        public async Task<GetRoomIdResponse> GetRoomIdFromAlias(string accessToken, string roomAlias, CancellationToken cancellationToken)
        {
            roomAlias = roomAlias.Replace("#", "%23");
            HttpClient httpClient = CreateHttpClient(accessToken);
            var path = $"{ResourcePath}/directory/room/{roomAlias}";
            
            return await httpClient.GetAsJsonAsync<GetRoomIdResponse>(path, cancellationToken);
        }


        public async Task<List<string>> GetJoinedRoomsAsync(string accessToken,
            CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);
            
            var path = $"{ResourcePath}/joined_rooms";
            var json = await httpClient.GetAsStringAsync(path, cancellationToken);
            var obj = JsonConvert.DeserializeObject<JoinedRoomsResponse>(json);
            return obj.JoinedRoomIds;
        }

        public async Task LeaveRoomAsync(string accessToken, string roomId,
            CancellationToken cancellationToken)
        {
            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/leave";

            await httpClient.PostAsync(path, cancellationToken);
        }

        public record RoomNameResponse
        {
            public string name;
        }
        public async Task<string> GetRoomNameAsync(string accessToken, string roomId, CancellationToken cancellationToken)
        {
            var path = $"{ResourcePath}/rooms/{roomId}/state/m.room.name/";
            HttpClient httpClient = CreateHttpClient(accessToken);
            var json = await httpClient.GetAsStringAsync(path, cancellationToken);
            var payload = JsonConvert.DeserializeObject<RoomNameResponse>(json);
            return payload.name;
        }
        
        public async Task<EventResponse> SetTopicAsync(string accessToken,
            string roomId,
            string topic, CancellationToken cancellationToken)
        {
            const string eventType = "m.room.topic";
            var model = new ChangeTopicRequest(topic);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/state/{eventType}";
            
            return await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
        }
        
        public async Task<EventResponse> SetNameAsync(string accessToken,
            string roomId,
            string name, CancellationToken cancellationToken)
        {
            const string eventType = "m.room.name";
            var model = new ChangeNameRequest(name);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/state/{eventType}";
            
            return await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
        }
        
        public async Task<EventResponse> SetAvatarAsync(string accessToken,
            string roomId,
            string url, CancellationToken cancellationToken)
        {
            const string eventType = "m.room.avatar";
            var model = new ChangeAvatarRequest(url);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/state/{eventType}";
            
            return await httpClient.PutAsJsonAsync<EventResponse>(path, model, cancellationToken);
        }
        
        public record PublicRoomResponse
        {
            public int TotalRoomCountEstimate;

            public record PublicRoom
            {
                public bool GuestCanJoin;
                public int NumJoinedMembers;
                public string Topic;
                public string AvatarUrl;
                public string RoomId;
                public string Name;
                public string CanonicalAlias;
                public bool WorldReadable;
            }
            public List<PublicRoom> chunk;
        }

        public async Task<ReadOnlyCollection<string>> GetPublicRoomIds(string accessToken,
            CancellationToken cancellationToken)
        {
            return (await GetPublicRooms(accessToken, cancellationToken)).Select(r => r.RoomId).ToList().AsReadOnly();
        }

        public async Task<ReadOnlyCollection<PublicRoomResponse.PublicRoom>> GetPublicRooms(string accessToken, CancellationToken cancellationToken)
        {
            int totalRooms = int.MaxValue;
            var allRooms = new List<PublicRoomResponse.PublicRoom>();
            int since = 0;
            while (allRooms.Count < totalRooms)
            {
                var response = await GetPublicRooms(accessToken, 50, since, cancellationToken);
                if (response.TotalRoomCountEstimate < totalRooms)
                {
                    totalRooms = response.TotalRoomCountEstimate;
                }

                if (response.chunk.Count > 0)
                {
                    allRooms.AddRange(response.chunk);
                }
                else
                {
                    break;
                }
            }
            return allRooms.AsReadOnly();
        }
        public async Task<PublicRoomResponse> GetPublicRooms(string accessToken, int limit, int since, CancellationToken cancellationToken)
        {
            var path = $"/{ResourcePath}/publicRooms?limit={limit}"; //&since={since}";
            HttpClient httpClient = CreateHttpClient(accessToken);
            return await httpClient.GetAsJsonAsync<PublicRoomResponse>(path, cancellationToken);
        }

        public async Task SetDisplayname(string accessToken, string roomId, string userId, string newNick, CancellationToken cancellationToken)
        {
            const string eventType = "m.room.member";
            var model = new ChangeRoomNickRequest(newNick);

            HttpClient httpClient = CreateHttpClient(accessToken);

            var path = $"{ResourcePath}/rooms/{roomId}/state/{eventType}/{HttpUtility.UrlEncode(userId)}";
            
            await httpClient.PutAsJsonAsync<object>(path, model, cancellationToken);
        }
    }
}