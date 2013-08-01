using System;

#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif

namespace pumpkin
{
    /// <summary>
    /// Represents data for the StorageDeviceSelected event.
    /// </summary>
    public class StorageDeviceActionEventArgs : EventArgs
    {
        /// <summary>The location of user storage.</summary>
        public readonly StorageContainer StorageContainer;

        /// <summary>The action the user took to close the storage device.</summary>
        public readonly DialogAction DialogAction;

        /// <summary>
        /// Initialises a new instance of a <see cref="StorageDeviceActionEventArgs">StorageDeviceActionEventArgs</see>.
        /// </summary>
        /// <param name="storageContainer">A reference to the storage container.</param>
        /// <param name="dialogAction">The action the user took to close the storage device.</param>
        public StorageDeviceActionEventArgs(StorageContainer storageContainer, DialogAction dialogAction)
        {
            this.StorageContainer = storageContainer;
            this.DialogAction = dialogAction;
        }
    }
}
