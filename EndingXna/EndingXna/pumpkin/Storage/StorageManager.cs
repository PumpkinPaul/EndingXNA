using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using KiloWatt.Runtime.Support;
using Microsoft.Xna.Framework.Storage;

namespace pumpkin
{
    public class StorageManager
    {
        private struct SaveInfo 
        {
            public object Data { get; set; }
            public string Filename { get; set; }
        }

        private Queue<SaveInfo> _saveQueue = new Queue<SaveInfo>();

        private StorageContainer _storageContainer;
        
        /// <summary>
        /// Delegate backing the StorageDeviceSelected event.
        /// </summary>
        private EventHandler<StorageDeviceActionEventArgs> _storageDeviceActionEvent;

        /// <summary>
        /// Lock for StorageDeviceSelected delegate access.
        /// </summary>
        readonly object _storageDeviceActionLock = new object();

        /// <summary>
        /// Occurs when a storage device is selected.
        /// </summary>
        public event EventHandler<StorageDeviceActionEventArgs> StorageDeviceAction
        {
            add { lock (this._storageDeviceActionLock) { this._storageDeviceActionEvent += value; } }
            remove { lock (this._storageDeviceActionLock) { this._storageDeviceActionEvent -= value; } }
        }

        /// <summary>A collection of serializers.</summary>
        private readonly Dictionary<string, XmlSerializer> _xmlSerializers = new Dictionary<string, XmlSerializer>();

        /// <summary>A reference to the storage device.</summary>
        private StorageDevice _storageDevice;

        /// <summary>The result of the stroage guide selection.</summary>
        private IAsyncResult _storageGuideResult;

        /// <summary>Whether to show the storage device selection guide.</summary>
        private bool _showStorageGuide;

        /// <summary>Whether the storage device selection guide has been requested.</summary>
        private bool _storageGuideRequested;

        /// <summary>Whether saving is enabled.</summary>
        public bool StorageEnabled { get; private set; }

        public string ContainerName { get; private set; }

        public StorageManager(string gameName)
        {
            this.ContainerName = gameName + " Saved Data";
        }


        /// <summary>
        /// Updates the storage manager.
        /// </summary>
        public void Update()
        {
            if (this._storageContainer != null && this._storageContainer.IsDisposed) {
                this._storageContainer = null;    
            }

            //Get a storage device?
            if (this.GetStorageDevice())
            {
                //Raise the event
                this.NotifyStorageDeviceAction();
            }

            MaybeSaveDataInternal();
         }

        /// <summary>
        /// Shows the storage guide. 
        /// </summary>
        public void ShowStorageGuide()
        {
            this._showStorageGuide = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public T Load<T>(string filename) {
            
            var obj = default(T);

            try
            {
                if (this.StorageEnabled == false)
                    return obj;

                if (this._storageDevice == null)
                    return obj;

                if (this._storageDevice.IsConnected == false)
                    return obj;

                using (var storageContainer = this.OpenStorageContainer(this._storageDevice))
                    obj = Load<T>(storageContainer, filename); 
            }
            catch
            {
                //Catch the error and swallow it! Nom nom.
            }

            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storageContainer"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public T Load<T>(StorageContainer storageContainer, string filename) {
            
            var obj = default(T);

            try
            {
                if (this.StorageEnabled == false)
                    return obj;

                if (this._storageDevice == null)
                    return obj;

                if (this._storageDevice.IsConnected == false)
                    return obj;

                if (storageContainer.FileExists(filename) == false)
                    return obj;

                using (var fileStream = storageContainer.OpenFile(filename, FileMode.Open))
                {
                    var x = GetSerializer(typeof(T));
                    obj = (T)x.Deserialize(fileStream);
                }
            }
            catch 
            {
                //Catch the error and swallow it! Nom nom.
            }

            return obj;
        }

        public T DeserializeObject<T>(string data)
        {
            using (var tr = new StringReader(data))
            {
                var x = GetSerializer(typeof(T));
                return (T)x.Deserialize(tr);
            }
        }

        /// <summary>
        /// Saves the data to the storage device.
        /// </summary>
        /// <param name="data">The object to save.</param>
        /// <param name="filename">Name of the file to save.</param>
        public void Save(object data, string filename)
        {
            try
            {
                if (this.StorageEnabled == false)
                    return;

                if (this._storageDevice == null)
                    return;

                if (this._storageDevice.IsConnected == false)
                    return;

                //Push a new packet onto the queue
                lock(_saveQueue) {
                    _saveQueue.Enqueue(new SaveInfo {
                        Data = data,
                        Filename = filename
                    });
                }
            }
            catch 
            {
                //Catch the error and swallow it! Nom nom.
            }
        }

        private void MaybeSaveDataInternal() {
            
            try
            {
                if (this.StorageEnabled == false)
                    return;

                if (this._storageDevice == null)
                    return;

                if (this._storageDevice.IsConnected == false)
                    return;

                var saveRequired = false;
                var info = new SaveInfo();
                lock(_saveQueue) 
                {
                    saveRequired = _saveQueue.Count > 0; 
                }

                if (saveRequired == false)
                    return;

                #if WINDOWS || XBOX360
                //Sync the cached file system (asynchronously)
                XnaGame.Instance.ThreadPoolComponent.AddTask(ThreadTarget.Core1Thread3, SaveQueuedData, null, null);
                #elif WINDOWS_PHONE
                SaveQueuedData();
                #endif
            }
            catch
            {
                //Catch the error and swallow it! Nom nom.
            }
        }

        /// <summary>
        /// Gets a storage device.
        /// </summary>
        /// <returns>True if a stroage device was selected.</returns>
        private bool GetStorageDevice()
        {
            try
            {
                //Being showing the storage device selection guide if it is required.
                this.ShowDeviceSelector();

                //Monitor guide for a selection.
                return this.MonitorDeviceSelection();
            }
            catch (Exception ex)
            {
                this._storageDevice = null;
                this._showStorageGuide = false;
                this._storageGuideRequested = false;
                this.StorageEnabled = false;

                return false;
            }
        }

        /// <summary>
        /// Shows the storage device selector guide if required.
        /// </summary>
        private void ShowDeviceSelector()
        {
#if !PERFHUD
            //Begin showing the storage guide asynchornously.
            if (!this._showStorageGuide || Microsoft.Xna.Framework.GamerServices.Guide.IsVisible)
                return;
#else
            if (!this._showStorageGuide)
                return;
#endif

            this._storageGuideResult = StorageDevice.BeginShowSelector(null, null);

            this._storageGuideRequested = true;
            this._showStorageGuide = false;
        }

        /// <summary>
        /// Monitors the storage device guide for a selection. 
        /// </summary>
        /// <returns>True if storage guide dialog is dismissed (select or back); otherwise false.</returns>
        private bool MonitorDeviceSelection()
        {
            bool guideDismissed = false;

            //Monitor for a device selection
            if (this._storageGuideRequested && this._storageGuideResult.IsCompleted)
            {
                this._storageDevice = StorageDevice.EndShowSelector(this._storageGuideResult);
                if (this._storageDevice != null && this._storageDevice.IsConnected)
                    this.StorageEnabled = true;

                guideDismissed = true;

                this._storageGuideRequested = false;
            }

            return guideDismissed;
        }

        /// <summary>
        /// Notifies interested parties of a storage device selection / cancellation.
        /// </summary>
        private void NotifyStorageDeviceAction()
        {
            if (this.StorageEnabled)
            {
                using (var storageContainer = this.OpenStorageContainer(this._storageDevice))
                    this.OnStorageDeviceSelected(new StorageDeviceActionEventArgs(storageContainer, DialogAction.Select));
            }
            else
                //Raise the event so interested parties know the device selection screen was cancelled.
                this.OnStorageDeviceSelected(new StorageDeviceActionEventArgs(null, DialogAction.Back));
        }

        /// <summary>
        /// Gets the xml serializer for the passed type.
        /// </summary>
        /// <param name="type">The type of xml serializer.</param>
        /// <returns>An xml serializer.</returns>
        /// <remarks>Caches the serializer in a dictionary.</remarks>
        private XmlSerializer GetSerializer(Type type)
        {
            if (this._xmlSerializers.ContainsKey(type.Name) == false)
                this._xmlSerializers[type.Name] = new XmlSerializer(type);

            return this._xmlSerializers[type.Name];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SaveQueuedData()
        {
            SaveInfo info;
            lock (_saveQueue)
            {
                info = _saveQueue.Dequeue();
            }

            var type = info.Data.GetType();

            using (var storageContainer = this.OpenStorageContainer(this._storageDevice))
            {
                using (var fileStream = storageContainer.OpenFile(info.Filename, FileMode.Create))
                {
                    var x = this.GetSerializer(type);
                    x.Serialize(fileStream, info.Data);
                }  
            }
        }

        public StorageContainer OpenStorageContainer() 
        {
            return this.OpenStorageContainer(this._storageDevice);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageDevice"></param>
        /// <returns></returns>
        private StorageContainer OpenStorageContainer(StorageDevice storageDevice)
        {
            //Got one already?
            if (this._storageContainer != null && this._storageContainer.IsDisposed == false)
                return this._storageContainer;

            //Open a storage container.
            IAsyncResult result = storageDevice.BeginOpenContainer(this.ContainerName, null, null);

            //Wait for the WaitHandle to become signaled.
            #if !WINDOWS_PHONE
            result.AsyncWaitHandle.WaitOne();
            #endif

            this._storageContainer = storageDevice.EndOpenContainer(result);

            //Close the wait handle.
            #if !WINDOWS_PHONE
            result.AsyncWaitHandle.Close();
            #endif

            return this._storageContainer;
        }

        /// <summary>
        /// Occurs when a storage device is selected.
        /// </summary>
        /// <param name="e">A StorageDeviceActionEventArgs containing the data for the event.</param>
        protected internal void OnStorageDeviceSelected(StorageDeviceActionEventArgs e)
        {
            //Now raise the event to any subscribers
            EventHandler<StorageDeviceActionEventArgs> handler;

            lock (this._storageDeviceActionLock)
                handler = this._storageDeviceActionEvent;

            if (handler != null)
                handler(this, e);
        }
    }
}
