using System.Threading.Tasks;
using Windows.Media.Control;

namespace MusicThingWindows
{

    class WindowsMedia
    {
        private GlobalSystemMediaTransportControlsSession? session = null;

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