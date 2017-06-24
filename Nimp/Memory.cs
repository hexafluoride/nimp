using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class Memory
    {
        public byte[] Buffer = new byte[2 >> 26];

        public Memory()
        {

        }

        public uint ReadWord(uint location)
        {
            return Buffer[location] |
                ((uint)Buffer[location + 1] << 8) |
                ((uint)Buffer[location + 2] << 16) |
                ((uint)Buffer[location + 3] << 24);
        }

        public void WriteWord(uint word, uint location)
        {
            Buffer[location] = (byte)(word & 0xFF);
            Buffer[location + 1] = (byte)((word & 0xFF00) >> 8);
            Buffer[location + 2] = (byte)((word & 0xFF0000) >> 16);
            Buffer[location + 3] = (byte)((word & 0xFF000000) >> 24);
        }

        public long GetOpcode(uint word)
        {
            return (word & (0x3F << 26)) >> 26;
        }

        public long GetS(uint word)
        {
            return (word & (0x1F << 21)) >> 21;
        }

        public long GetT(uint word)
        {
            return (word & (0x1F << 16)) >> 16;
        }

        public long GetD(uint word)
        {
            return (word & (0x1F << 11)) >> 11;
        }

        public long GetFunc(uint word)
        {
            return word & (0x3F);
        }

        public void DumpInstruction(uint i)
        {
            Console.WriteLine("opcode: 0x{0:X2}, $s: ${1:00}, $t: ${2:00}, $d: ${3:00}, func: 0x{4:X2}",
                GetOpcode(i),
                GetS(i),
                GetT(i),
                GetD(i),
                GetFunc(i));
        }
    }
}
