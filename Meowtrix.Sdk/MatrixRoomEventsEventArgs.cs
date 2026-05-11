using System;
using System.Collections.Generic;
using Meowtrix.Sdk.Core.Domain.RoomEvent;

namespace Meowtrix.Sdk
{
    public class MatrixRoomEventsEventArgs : EventArgs
    {
        public MatrixRoomEventsEventArgs(List<BaseRoomEvent> matrixRoomEvents, string nextBatch)
        {
            MatrixRoomEvents = matrixRoomEvents;
            NextBatch = nextBatch;
        }

        public List<BaseRoomEvent> MatrixRoomEvents { get; }
        
        public string NextBatch { get; }
    }
}