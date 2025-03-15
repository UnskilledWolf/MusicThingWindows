using System.Threading.Tasks;

namespace MusicThingWindows
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            WindowsMedia windowsMedia = new();
            var data = await windowsMedia.Retrieve();
            if (data != null)
                Console.WriteLine($"{data.Title}");
        }
    }
}