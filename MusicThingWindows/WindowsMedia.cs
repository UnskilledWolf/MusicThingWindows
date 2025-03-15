using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Control;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MusicThingWindows
{
    public delegate void MetadataUpdate();

    class WindowsMedia
    {
        private GlobalSystemMediaTransportControlsSession? session = null;

        public event MetadataUpdate? OnMetadataChanged;

        public bool IsInitialized
        {
            get { return session != null; }
        }

        // Constructs from current media data 
        public WindowsMedia()
        {
            InitializeSession();
        }

        private async void InitializeSession()
        {
            GlobalSystemMediaTransportControlsSessionManager sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            GlobalSystemMediaTransportControlsSession currentSession = sessionManager.GetCurrentSession();
            session = currentSession;

            currentSession.MediaPropertiesChanged += new TypedEventHandler<GlobalSystemMediaTransportControlsSession, MediaPropertiesChangedEventArgs>(RelayMediaDataChangedEvent);
        }

        private void RelayMediaDataChangedEvent(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            OnMetadataChanged?.Invoke();
        }

        public async Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> Retrieve()
        {
            if (session != null)
            {
                GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await session.TryGetMediaPropertiesAsync();
                return mediaProperties;
            }
            else
                return null;
        }

        public async Task<byte[]> GetCoverArt()
        {
            if (session != null)
            {
                GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await session.TryGetMediaPropertiesAsync();
                IRandomAccessStreamReference thumbnailStreamReference = mediaProperties.Thumbnail;
                return await ConvertToByteArrayAsync(thumbnailStreamReference);
            }
            else
                return [];
        }

        private static async Task<byte[]> ConvertToByteArrayAsync(IRandomAccessStreamReference streamReference)
        {
            if (streamReference == null)
                return [];

            using IRandomAccessStream stream = await streamReference.OpenReadAsync();
            using var reader = new DataReader(stream);
            await reader.LoadAsync((uint)stream.Size);
            byte[] buffer = new byte[stream.Size];
            reader.ReadBytes(buffer);
            return buffer;
        }
    }
}