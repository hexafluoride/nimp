using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nimp
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Utilities.OpcodeList = Enum.GetNames(typeof(Opcodes)).Concat(Enum.GetNames(typeof(AluFuncs))).Select(s => s.ToLower()).ToList();
            Utilities.CommandHelp["help"] = Utilities.CommandHelp["help"] + (string.Join("\n", Utilities.AutocompleteCommands.Select(s => "\t" + s + "\t\t" + Utilities.CommandHelp[s].Split('\n')[2])));
            Memory.Init();

            var reader = new StreamReader("./mips.hex");

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
                        Memory.WriteWord(Convert.ToUInt32(words[i + 1], 16), start + (i * 4));
                }
                catch
                {
                }
            }

            Task.Factory.StartNew(State.Loop);

            ulong prev_count = 0;

            do
            {
                ulong count = State.Count;
                ulong count_delta = count - prev_count;
                Console.Title = string.Format("{0:0.00} MIPS, cycle {1}", (count_delta / 500000d), count);
                prev_count = count;
            } while (!State.StoppedSemaphore.WaitOne(500));

            Console.Title += " - stopped execution";

            Console.ReadLine();
        }
    }
}
