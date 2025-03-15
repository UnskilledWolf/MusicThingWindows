using System.IO;

namespace MusicThingWindows
{
    public class PacketSerializer
    {
        private const char SegmentSeparator = ':';
        private readonly StringWriter _sw = new();
        private bool _firstWrite = true;

        public override string ToString()
        {
            return _sw.ToString();
        }

        public void Write(int s)
        {
            NextSegment();
            _sw.Write(s);
        }

        public void Write(string s)
        {
            Write(s.Length);
            NextSegment();
            _sw.Write(s);
        }

        public void Write(string[] ss)
        {
            Write(ss.Length);
            foreach (var s in ss) Write(s);
        }

        private void NextSegment()
        {
            if (!_firstWrite) _sw.Write(SegmentSeparator);
            _firstWrite = false;
        }
    }
}