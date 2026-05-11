using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meowtrix.Sdk.Core.Domain.MatrixRoom;
using Meowtrix.Sdk.Core.Domain.RoomEvent;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Login;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Create;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Join;
using Meowtrix.Sdk.Core.Infrastructure.Dto.User;

namespace Meowtrix.Sdk
{
    /// <summary>
    ///     A Client for interaction with Matrix.
    /// </summary>
    public interface IMatrixClient
    {
        string UserId { get; }

        Uri? BaseAddress { get; }

        bool IsLoggedIn { get; }
        
        bool IsSyncing { get; }
        
        string? Token { get; }

        MatrixRoom[] InvitedRooms { get; }

        MatrixRoom[] JoinedRooms { get; }

        MatrixRoom[] LeftRooms { get; }
        
        event EventHandler<MatrixRoomEventsEventArgs> OnMatrixRoomEventsReceived;

        Task<LoginResponse> LoginAsync(Uri baseAddress, string user, string password, string deviceId);
        Task LoginAsync(Uri baseAddress, string token, string? userId = null);

        void Start(string? nextBatch = null);

        void Stop();

        Task<CreateRoomResponse> CreateTrustedPrivateRoomAsync(string[] invitedUserIds);
        
        Task<string>  GetPublicRoomIdFromAlias(string roomAlias);
        
        MatrixRoom?  GetRoom(string roomId);

        Task<JoinRoomResponse> JoinTrustedPrivateRoomAsync(string roomId);

        Task<string> SendMessageAsync(string roomId, string message, string replyTo=null);
        Task<string> SendImageAsync(string roomId, string filename, byte[] imageData, string replyToEventId=null);
        Task<string> SendFileAsync(string roomId, string filename, byte[] blob);

        Task<List<string>> GetJoinedRoomsIdsAsync();

        Task LeaveRoomAsync(string roomId);
        
        Task<List<BaseRoomEvent>> GetHistory(string roomId, Func<BaseRoomEvent, Task<bool>> stopCallback);
        Task<List<BaseRoomEvent>> GetHistory(string roomId, string fromEventId, Func<BaseRoomEvent, Task<bool>> stopCallback);

        Task<string> EditMessage(string roomId, string messageId, string newText);
        Task SendTypingSignal(string roomId, TimeSpan timeout);
        Task SendTypingSignal(string roomId, bool isTyping);

        Task<string> GetString(string url);
        Task<string> GetRoomName(string id);
        Task<EventResponse> SetRoomTopicAsync(string roomId, string topic);
        Task<EventResponse> SetRoomNameAsync(string roomId, string name);
        Task<EventResponse> SetRoomAvatarAsync(string roomId, string url);
        Task<MatrixProfile> GetUserProfile(string fullUserId);
        Task<byte[]> GetMxcImage(string mxcUrl);
        Task<BaseRoomEvent> GetEvent(string eventId);
    }
}