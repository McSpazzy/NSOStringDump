using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NSOStringDump
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            var path = args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine($"{path} Does Not Exist.");
                return;
            }

            var complete = args.ToList().Contains("--complete");
            var sort = args.ToList().Contains("--sort");

            DoRip(path, $"{path}.strDump", complete, sort);
        }

        public static void DoRip(string path, string output, bool complete, bool sort)
        {
            var lines = File.ReadAllBytes(path);
            var ms = new MemoryStream();

            var isStarted = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var b = lines[i];

                if (!complete)
                {
                    if (!isStarted)
                    {
                        if (lines[i] == 0x65 && lines[i + 1] == 0x6E && lines[i + 2] == 0x64 && lines[i + 3] == 0x00)
                        {
                            isStarted = true;
                        }
                    }

                    if (!isStarted)
                    {
                        continue;
                    }
                }

                if (b == 0x0)
                {
                    ms.Write(new[] { (byte)'\r', (byte)'\n' }, 0, 2);
                    continue;
                }

                ms.WriteByte(b);

                if (complete)
                {
                    continue;
                }

                if (lines[i] == 0x48 && lines[i + 1] == 0xE1 && lines[i + 2] == 0x7A && lines[i + 3] == 0x3F && lines[i + 4] == 0x04)
                {
                    break;
                }
            }

            var srt = Encoding.UTF8.GetString(ms.ToArray()).Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            var lineDump = complete ? srt : srt.Where(s => !s.Any(d => d < 32)).Where(Clean).Distinct();
            if (sort)
            {
                lineDump = lineDump.OrderBy(s => s);
            }

            File.WriteAllLines(output, lineDump);
        }

        private static bool Clean(string val) => !val.Contains("�");
    }
}
