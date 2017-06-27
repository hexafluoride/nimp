using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class Memory
    {
        public byte[] Buffer = new byte[2 << 24];

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
    }
}
