using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Meowtrix.Sdk.Core.Domain;
using Meowtrix.Sdk.Core.Domain.MatrixRoom;
using Meowtrix.Sdk.Core.Domain.RoomEvent;
using Meowtrix.Sdk.Core.Domain.Services;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Login;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Create;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Join;
using Meowtrix.Sdk.Core.Infrastructure.Dto.User;
using Meowtrix.Sdk.Core.Infrastructure.Services;

namespace Meowtrix.Sdk
{
    /// <summary>
    ///     A Client for interaction with Matrix.
    /// </summary>
    public class MatrixClient : IMatrixClient
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly IPollingService _pollingService;
        
        private readonly UserService _userService;
        private readonly RoomService _roomService;
        private readonly EventService _eventService;
        private readonly MediaService _mediaService;
        
       
        public string? accessToken;
        private ulong _transactionNumber;

        public MatrixClient(
            IPollingService pollingService,
            UserService userService, 
            RoomService roomService, 
            EventService eventService,
            MediaService mediaService)
        {
            _pollingService = pollingService;
            _userService = userService;
            _roomService = roomService;
            _eventService = eventService;
            _mediaService = mediaService;
        }

        public event EventHandler<MatrixRoomEventsEventArgs> OnMatrixRoomEventsReceived;

        public string UserId { get; private set; }
        public string Homeserver { get; private set; }

        public Uri? BaseAddress { get; private set; }

        public bool IsLoggedIn { get; private set; }
        
        public bool IsSyncing { get; private set; }
        
        public string? Token
        {
            get => accessToken;
            set => accessToken = value;
        }

        public MatrixRoom[] InvitedRooms => _pollingService.InvitedRooms;
        
        public MatrixRoom[] JoinedRooms => _pollingService.JoinedRooms;

        public MatrixRoom[] LeftRooms => _pollingService.LeftRooms;

        public async Task<LoginResponse> LoginAsync(Uri baseAddress, string user, string password, string deviceId)
        {
            _userService.BaseAddress = baseAddress;
            _roomService.BaseAddress = baseAddress;
            _eventService.BaseAddress = baseAddress;
            _mediaService.BaseAddress = baseAddress;
            BaseAddress = baseAddress;
            
            LoginResponse response = await _userService.LoginAsync(user, password, deviceId, _cts.Token);

            UserId = response.UserId;
            Homeserver = response.HomeServer;
            
            accessToken = response.AccessToken;

            _pollingService.Init(baseAddress, accessToken);

            IsLoggedIn = true;

            return response;
        }

        public async Task LoginAsync(Uri baseAddress, string token, string? userId = null)
        {
            _userService.BaseAddress = baseAddress;
            _roomService.BaseAddress = baseAddress;
            _eventService.BaseAddress = baseAddress;
            _mediaService.BaseAddress = baseAddress;
            BaseAddress = baseAddress;

            UserId = userId;
            
            accessToken = token;

            _pollingService.Init(baseAddress, accessToken);

            IsLoggedIn = true;
        }

        public void Start(string? nextBatch = null)
        {
            if (!IsLoggedIn)
                throw new Exception("Call LoginAsync first");

            _pollingService.OnSyncBatchReceived += OnSyncBatchReceived;
            _pollingService.Start(nextBatch);

            IsSyncing = _pollingService.IsSyncing;
        }

        public void Stop()
        {
            _pollingService.Stop();
            _pollingService.OnSyncBatchReceived -= OnSyncBatchReceived;

            IsSyncing = _pollingService.IsSyncing;
        }
        
        public MatrixRoom? GetRoom(string roomId)
        {
            return _pollingService.GetMatrixRoom(roomId);
        }

        public async Task<CreateRoomResponse> CreateTrustedPrivateRoomAsync(string[] invitedUserIds) =>
            await _roomService.CreateRoomAsync(accessToken!, invitedUserIds, _cts.Token);
        
        public async Task<JoinRoomResponse> JoinTrustedPrivateRoomAsync(string roomId)
        {
            MatrixRoom? matrixRoom = _pollingService.GetMatrixRoom(roomId);
            if (matrixRoom != null && matrixRoom.Status != MatrixRoomStatus.Invited)
                return new JoinRoomResponse(matrixRoom.Id);

            return await _roomService.JoinRoomAsync(accessToken!, roomId, _cts.Token);
        }

        public async Task<string> GetPublicRoomIdFromAlias(string roomAlias)
        {
            GetRoomIdResponse response = await _roomService.GetRoomIdFromAlias(accessToken!, roomAlias, _cts.Token);

            return response.RoomId; //+ roomDomain;
        }
        
        public async Task<RoomService.InviteResult> InviteToRoomAsync(string roomId, string invitedId)
        {
            
            return await _roomService.InviteToRoomAsync(accessToken!, UserId, roomId, invitedId, _cts.Token);
        }

        public async Task<string> SendMessageAsync(string roomId, string message, string replyToEventId=null)
        {
            string transactionId = CreateTransactionId();
            
            EventResponse eventResponse = await _eventService.SendMessageAsync(accessToken!,
                roomId, transactionId, message, replyToEventId, _cts.Token);

            if (eventResponse.EventId == null)
                throw new NullReferenceException(nameof(eventResponse.EventId));

            return eventResponse.EventId;
            
        }

        public async Task<string> SendReactionAsync(string roomId, string reaction, string replyToEventId)
        {
            string transactionId = CreateTransactionId();

            EventResponse eventResponse = await _eventService.SendReactionAsync(accessToken!,
                roomId, transactionId, reaction, replyToEventId, _cts.Token);

            if (eventResponse.EventId == null)
                throw new NullReferenceException(nameof(eventResponse.EventId));

            return eventResponse.EventId;
        }

        public async Task<string> SendImageAsync(string roomId, string filename, byte[] imageData, string replyToEventId=null)
        {
            string transactionId = CreateTransactionId();
            
            var mxcUrl = await _mediaService.UploadImage(accessToken!, filename, imageData, _cts.Token);
            
            EventResponse eventResponse = await _eventService.SendImageAsync(accessToken!,
                roomId, transactionId, filename, mxcUrl, replyToEventId, _cts.Token);

            if (eventResponse.EventId == null)
                throw new NullReferenceException(nameof(eventResponse.EventId));

            return eventResponse.EventId;
        }

        public async Task<string> SendFileAsync(string roomId, string filename, byte[] blob)
        {
            string transactionId = CreateTransactionId();

            var mxcUrl = await _mediaService.UploadFile(accessToken!, filename, blob, _cts.Token);

            EventResponse eventResponse = await _eventService.SendFileAsync(accessToken!,
                roomId, transactionId, filename, mxcUrl, _cts.Token);

            if (eventResponse.EventId == null)
                throw new NullReferenceException(nameof(eventResponse.EventId));

            return eventResponse.EventId;
        }

        public async Task<string> GetString(string url)
        {
            return await _eventService.GetString(accessToken!, url, _cts.Token);
        }

        public async Task<List<string>> GetJoinedRoomsIdsAsync()
        {
            return await _roomService.GetJoinedRoomsAsync(accessToken!, _cts.Token);
        }

        public async Task LeaveRoomAsync(string roomId) => 
            await _roomService.LeaveRoomAsync(accessToken!, roomId, _cts.Token);

        public async Task<List<BaseRoomEvent>> GetHistory(string roomId,
            Func<BaseRoomEvent, Task<bool>> stopCallback)
        {
            return await GetHistory(roomId, null, stopCallback);
        }
        
        public async Task<List<BaseRoomEvent>> GetHistory(string roomId, string fromEventId, Func<BaseRoomEvent, Task<bool>> stopCallback)
        {
            return await _eventService.GetTimelineEventsAsync(accessToken!, roomId, null, stopCallback, _cts.Token);
        }

        public async Task<string> EditMessage(string roomId, string messageId, string newText)
        {
            string transactionId = CreateTransactionId();
            
            EventResponse eventResponse = await _eventService.EditMessageAsync(accessToken!,
                roomId, transactionId, messageId, newText, _cts.Token);

            if (eventResponse.EventId == null)
                throw new NullReferenceException(nameof(eventResponse.EventId));

            return eventResponse.EventId;
        }

        private void OnSyncBatchReceived(object? sender, SyncBatchEventArgs syncBatchEventArgs)
        {
            if (sender is not IPollingService)
                throw new ArgumentException("sender is not polling service");

            SyncBatch batch = syncBatchEventArgs.SyncBatch;
            
            OnMatrixRoomEventsReceived.Invoke(this, new MatrixRoomEventsEventArgs(batch.MatrixRoomEvents,  batch.NextBatch));
        }

        private string CreateTransactionId()
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            ulong counter = _transactionNumber;

            _transactionNumber += 1;

            return $"m{timestamp}.{counter}";
        }

        public async Task SendTypingSignal(string roomId, TimeSpan timeout)
        {
            await _eventService.SendTypingSignalAsync(accessToken!, roomId, UserId, timeout, _cts.Token);
        }
        
        public async Task SendTypingSignal(string roomId, bool isTyping)
        {
            await _eventService.SendTypingSignalAsync(accessToken!, roomId, UserId, isTyping, _cts.Token);
        }
        public async Task<ReadOnlyCollection<string>> GetPublicRoomIds()
        {
            return await _roomService.GetPublicRoomIds(accessToken!, _cts.Token);
        }
        public async Task<ReadOnlyCollection<RoomService.PublicRoomResponse.PublicRoom>> GetPublicRooms()
        {
            return await _roomService.GetPublicRooms(accessToken!, _cts.Token);
        }

        public async Task<string> GetRoomName(string roomId)
        {
            return await _roomService.GetRoomNameAsync(accessToken!, roomId, _cts.Token);
        }
        
        public async Task<EventResponse> SetRoomTopicAsync(string roomId, string topic)
        {
            var transactionId = CreateTransactionId();
            return await _roomService.SetTopicAsync(accessToken!, roomId, topic, _cts.Token);
        }
        public async Task<EventResponse> SetRoomAvatarAsync(string roomId, string url)
        {
            var transactionId = CreateTransactionId();
            return await _roomService.SetAvatarAsync(accessToken!, roomId, url, _cts.Token);
        }
        public async Task<EventResponse> SetRoomNameAsync(string roomId, string name)
        {
            var transactionId = CreateTransactionId();
            return await _roomService.SetNameAsync(accessToken!, roomId, name, _cts.Token);
        }
        public async Task<MatrixProfile> GetUserProfile(string userId)
        {
            return await _userService.GetProfile(accessToken!, userId, _cts.Token);
        }

        public async Task<byte[]> GetMxcImage(string mxcUrl)
        {
            return await _mediaService.GetMedia(accessToken!, mxcUrl, _cts.Token);
        }
        
        public async Task<BaseRoomEvent> GetEvent(string eventId)
        {
            return await _eventService.GetEvent(accessToken!, eventId, _cts.Token);
        }

        public async Task SetNickname(string newNick, string roomId=null)
        {
            if (roomId == null)
            {
                await _userService.SetNickname(accessToken!, UserId, newNick, _cts.Token);
            }
            else
            {
                await _roomService.SetDisplayname(accessToken!, roomId, UserId, newNick, _cts.Token);
            }
        }
    }
}