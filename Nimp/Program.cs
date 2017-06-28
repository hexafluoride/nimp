using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nimp
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new StreamReader("./mips.hex");

            State s = new State();

            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (!line.Contains(" "))
                    continue;

                try
                {
                    var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string a = line.Split(' ')[0].Trim('[', ']');

                    uint start = Convert.ToUInt32(a, 16);
                    uint test = 0;
                    int count = uint.TryParse(words[2], NumberStyles.HexNumber, null, out test) ? 4 : 1;
                    
                    for (uint i = 0; i < 4; i++)
                        s.Memory.WriteWord(Convert.ToUInt32(words[i + 1], 16), start + (i * 4));
                }
                catch
                {
                }
            }

            Task.Factory.StartNew(s.Loop);

            ulong last_count = 0;

            while(true)
            {
                Thread.Sleep(1000);
                ulong count = s.Count;
                //Console.WriteLine("{0:0.00} MIPS", (count - last_count) / 1000000d);
                last_count = count;
            }
        }
    }
}
