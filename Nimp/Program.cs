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
            uint addi1 = 0x20010002;
            uint addi2 = 0x2002000F;
            uint addu = 0x22f821; // 0000 0000 0010 0010 1111 1000 0010 0001
                                  // 000000 00001 00010 11111 00000 100001
                                  // opcode  $s    $t    $d   shift  func

            State s = new State();
            s.Memory.WriteWord(addi1, 0);
            s.Memory.WriteWord(addi2, 4);
            s.Memory.WriteWord(addu, 8);

            s.Step();
            s.Step();
            s.Step();

            Console.ReadLine();
        }
    }
}
