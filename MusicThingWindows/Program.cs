using System.Threading.Tasks;

namespace MusicThingWindows
{
    class Program
    {
        public static void Main(string[] args)
        {
            Server server = new();
            server.Start();
            Console.Read();
        }
    }
}