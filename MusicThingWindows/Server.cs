using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Windows.Media.Control;

namespace MusicThingWindows
{
    class Server
    {
        public static Server? serverInstance;

        private const int PORT = 18423;
        private static readonly Random rng = new Random();

        private readonly HttpServer server;
        private readonly WindowsMedia windowsMedia;

        public Server()
        {
            server = new HttpServer(IPAddress.Any, PORT);
            windowsMedia = new();

            server.OnGet += (sender, args) =>
            {
                args.Response.StatusCode = 404;
                args.Response.Close();
            };
            server.AddWebSocketService<MusicThingWebsocketBehavior>("/ws");

            windowsMedia.OnMetadataChanged += new MetadataUpdate(BroadcastMetadata);

            serverInstance = this;
        }

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }


        public async Task<string> SerializeMetadata()
        {
            GlobalSystemMediaTransportControlsSessionMediaProperties? data = await windowsMedia.Retrieve();
            if (data != null)
            {
                PacketSerializer packetSerializer = new();
                packetSerializer.Write(data.Title);
                packetSerializer.Write(data.Artist);
                packetSerializer.Write(data.AlbumTitle);
                packetSerializer.Write("https://0.0.0.0");

                return packetSerializer.ToString();
            }
            else
                return "";
        }

        public void BroadcastMetadata()
        {
            string serializedData = SerializeMetadata().Result;
            server.WebSocketServices.Broadcast(serializedData);
        }

        private class MusicThingWebsocketBehavior : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                if (serverInstance == null)
                    return;

                Debug.WriteLine($"WebSocket client connected from {Context.UserEndPoint}");
                string serializedData = serverInstance.SerializeMetadata().Result;

                Send(serializedData);
            }
        }

    }
}