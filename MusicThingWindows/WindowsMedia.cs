using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Control;

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
    }
}