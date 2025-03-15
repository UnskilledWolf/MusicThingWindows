using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Windows.Media.Control;
using System.Text.RegularExpressions;
using System.Text;

namespace MusicThingWindows
{
    class Server
    {
        public static Server? serverInstance;

        private const int PORT = 18423;
        private static readonly Random rng = new();

        private readonly HttpServer server;
        private readonly WindowsMedia windowsMedia;
        private readonly Regex coverArtRegex = new(@"^\/cover_art\/.*$");

        public Server()
        {
            server = new HttpServer(IPAddress.Any, PORT);
            windowsMedia = new();

            server.OnGet += (sender, args) =>
            {
                Debug.WriteLine($"Received request from {args.Request.RemoteEndPoint}");

                var req = args.Request;
                var res = args.Response;

                if (coverArtRegex.IsMatch(req.RawUrl))
                {
                    var coverArt = windowsMedia.GetCoverArt().Result;
                    res.ContentLength64 = coverArt.Length;
                    res.Close(coverArt, true);
                }
                else
                {
                    res.StatusCode = 404;
                    res.Close();
                }

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
                packetSerializer.Write(NextCoverArtUrl());

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

        private static string NextCoverArtUrl()
        {
            return $"http://localhost:{PORT}/cover_art/{rng.Next():x8}";
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