using System;
using System.IO;
using System.Linq;
using System.Text;

namespace showbytes
{
    class Program
    {
        private const int DEFAULT_BYTES_PER_LINE = 24;
        private static int _bytesPerLine = DEFAULT_BYTES_PER_LINE;
        private static int _numBytes = int.MaxValue;
        private static int _bytesWritten = 0;
        private static StringBuilder _asciiLine = new StringBuilder();

        static void ShowHelp(int exitCode)
        {
            Console.WriteLine("USAGE:\n\nshowbytes [--num-bytes 500] [--bytes-per-line 24] file.bin");
            Console.WriteLine("Default: Show all bytes, with 24 bytes per line");
            Environment.Exit(exitCode);
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    ShowHelp(1);
                }

                if (args.Any(x => x == "--help"))
                {
                    ShowHelp(0);
                }

                int nbIdx = Array.IndexOf(args, "--num-bytes");
                if (nbIdx >= 0)
                {
                    _numBytes = int.Parse(args[nbIdx + 1]);
                }

                int bplIdx = Array.IndexOf(args, "--bytes-per-line");
                if (bplIdx >= 0)
                {
                    _bytesPerLine = int.Parse(args[bplIdx + 1]);
                }

                if (!File.Exists(args[args.GetUpperBound(0)]))
                {
                    Console.WriteLine($"File {args[0]} not found");
                    Environment.Exit(1);
                }

                var buffer = new byte[0x4000];
                void go()
                {
                    using (var fs = File.OpenRead(args[args.GetUpperBound(0)]))
                    {
                        int count;
                        while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < count; ++i)
                            {
                                WriteByte(buffer[i]);
                                if (++_bytesWritten >= _numBytes)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                go();

                // Last line
                if (_asciiLine.Length > 0)
                {
                    int numMissingChars = _bytesPerLine - _asciiLine.Length;
                    Console.Write(string.Join("", Enumerable.Range(0, numMissingChars).Select(x => "   ")) + "  ");
                    Console.WriteLine(_asciiLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ShowHelp(1);
            }

            Console.WriteLine("DONE");
            Console.Read();
        }

        static void WriteByte(byte b)
        {
            Console.Write($"{b.ToString("X2")} ");
            if (b >= 33 && b <= 126)
            {
                _asciiLine.Append(Encoding.ASCII.GetString(new[] { b }));
            }
            else
            {
                _asciiLine.Append(".");
            }
            if (_asciiLine.Length == _bytesPerLine)
            {
                Console.WriteLine($"  {_asciiLine}");
                _asciiLine.Clear();
            }
        }
    }
}
