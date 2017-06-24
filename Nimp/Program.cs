using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    class Program
    {
        static void Main(string[] args)
        {
            uint mflo = 0xf012;
            uint addu = 0x22f821; // 0000 0000 0010 0010 1111 1000 0010 0001
                                  // 000000 00001 00010 11111 00000 100001
                                  // opcode  $s    $t    $d   shift  func

            Memory m = new Memory();
            m.DumpInstruction(mflo);
            m.DumpInstruction(addu);

            Console.ReadLine();
        }
    }
}
