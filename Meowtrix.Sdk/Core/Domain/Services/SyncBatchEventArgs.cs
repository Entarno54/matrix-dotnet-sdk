using System;

namespace Meowtrix.Sdk.Core.Domain.Services
{
    public class SyncBatchEventArgs : EventArgs
    {
        public SyncBatchEventArgs(SyncBatch syncBatch)
        {
            SyncBatch = syncBatch;
        }

        public SyncBatch SyncBatch { get; }
    }
}