using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class Memory
    {
        public byte[][] Pages = new byte[2 << 20][];
        byte[] _cp;
        uint _cpid = uint.MaxValue;

        public uint CacheMisses = 0;
        public uint CacheHits = 0;

        public Memory()
        {

        }

        // TODO: Implement stack separately to improve performance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetPage(uint location)
        {
            uint pid = location >> 12;

            if (pid == _cpid)
            {
                CacheHits++;
                return _cp;
            }

            CacheMisses++;

            byte[] page = Pages[pid];

            _cpid = pid;

            if (page == null)
                return _cp = Pages[pid] = new byte[(2 << 12)];

            return _cp = page;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(uint location)
        {
            byte[] page = GetPage(location);
            return page[location & 0xFFF];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(uint location, byte v)
        {
            byte[] page = GetPage(location);
            page[location & 0xFFF] = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadWord(uint location)
        {
            byte[] page = GetPage(location);

            location &= 0xFFF;

            return page[location] |
                ((uint)page[location + 1] << 8) |
                ((uint)page[location + 2] << 16) |
                ((uint)page[location + 3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteWord(uint word, uint location)
        {
            byte[] page = GetPage(location);

            location &= 0xFFF;

            page[location] = (byte)(word & 0xFF);
            page[location + 1] = (byte)((word & 0xFF00) >> 8);
            page[location + 2] = (byte)((word & 0xFF0000) >> 16);
            page[location + 3] = (byte)((word & 0xFF000000) >> 24);
        }
    }
}
