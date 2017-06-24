﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            int counter = 0;

            while(!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                uint word = Convert.ToUInt32(line, 16);
                s.Memory.WriteWord(word, (uint)counter++ * 4);
            }

            Task.Factory.StartNew(s.Loop);

            ulong last_count = 0;

            while(true)
            {
                Thread.Sleep(1000);
                ulong count = s.Count;
                Console.WriteLine("{0:0.00} MIPS", (count - last_count) / 1000000d);
                last_count = count;
            }
        }
    }
}
