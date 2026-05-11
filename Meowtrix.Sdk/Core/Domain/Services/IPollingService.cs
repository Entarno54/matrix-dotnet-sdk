using System;

namespace Meowtrix.Sdk.Core.Domain.Services
{
    public interface IPollingService : IDisposable
    {
        MatrixRoom.MatrixRoom[] InvitedRooms { get; }

        MatrixRoom.MatrixRoom[] JoinedRooms { get; }

        MatrixRoom.MatrixRoom[] LeftRooms { get; }
        
        public bool IsSyncing { get; }
        
        public event EventHandler<SyncBatchEventArgs> OnSyncBatchReceived;

        void Init(Uri nodeAddress, string accessToken);

        void Start(string? nextBatch = null);

        void Stop();

        MatrixRoom.MatrixRoom? GetMatrixRoom(string roomId);
    }
}