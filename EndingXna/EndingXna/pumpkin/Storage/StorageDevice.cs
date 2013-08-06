using System;
using System.Threading;

#if WINDOWS_PHONE
namespace Microsoft.Xna.Framework.Storage
{
    /// <summary>
    /// Stub for Xbox style storage on Phone so that I can use the StorageManager
    /// </summary>
    public class StorageDevice
    {
        public class StorageAsyncResult : IAsyncResult 
        {
            public StorageAsyncResult()
            {
                IsCompleted = true;
            }

            public bool IsCompleted { get; private set; }
            public WaitHandle AsyncWaitHandle { get; private set; }
            public object AsyncState { get; private set; }
            public bool CompletedSynchronously { get; private set; }
        }

        private static readonly StorageDevice StaticStorageDevice = new StorageDevice();

        public bool IsConnected { get; set; } 

        public StorageDevice() 
        {
            IsConnected = true;
        }

        public IAsyncResult BeginOpenContainer(string containerName, AsyncCallback callback, object state)
        {
            return new StorageAsyncResult();
        }

        public StorageContainer EndOpenContainer(IAsyncResult result)
        {
            return new StorageContainer();
        }

        public static IAsyncResult BeginShowSelector(AsyncCallback callback, object state)
        {
            return new StorageAsyncResult();
        }

        public static StorageDevice EndShowSelector(IAsyncResult result)
        {
            return StaticStorageDevice;
        }
    }
}
#endif
