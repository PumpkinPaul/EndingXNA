using System;
using System.IO;
using System.IO.IsolatedStorage;

#if WINDOWS_PHONE
namespace Microsoft.Xna.Framework.Storage
{
    /// <summary>
    /// Stub for Xbox style storage on Phone so that I can use the StorageManager
    /// </summary>
    public class StorageContainer : IDisposable
    {
        private readonly IsolatedStorageFile _userStorage;

        public StorageContainer()
        {
            _userStorage = IsolatedStorageFile.GetUserStoreForApplication();    
        }

        public bool FileExists(string path)
        {
            return _userStorage.FileExists(path);
        }

        public Stream OpenFile(string path, FileMode fileMode)
        {
            return _userStorage.OpenFile(path, fileMode);
        }

        public void Dispose()
        {
            
        }

        public bool IsDisposed { get; set; } 
    }
}
#endif
