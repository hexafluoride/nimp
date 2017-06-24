using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class Utilities
    {
        public static void DumpInstruction(uint i)
        {
            Console.WriteLine("opcode: 0x{0:X2}, $s: ${1:00}, $t: ${2:00}, $d: ${3:00}, func: 0x{4:X2}",
                GetOpcode(i),
                GetS(i),
                GetT(i),
                GetD(i),
                GetFunc(i));
        }

        public static long GetOpcode(uint word)
        {
            return (word & (0x3F << 26)) >> 26;
        }

        public static long GetS(uint word)
        {
            return (word & (0x1F << 21)) >> 21;
        }

        public static long GetT(uint word)
        {
            return (word & (0x1F << 16)) >> 16;
        }

        public static long GetD(uint word)
        {
            return (word & (0x1F << 11)) >> 11;
        }

        public static long GetFunc(uint word)
        {
            return word & (0x3F);
        }
    }
}
