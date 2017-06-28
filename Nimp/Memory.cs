using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public static class Memory
    {
        public static byte[][] Pages = new byte[2 << 20][];
        static byte[] _cp;
        static uint _cpid = uint.MaxValue;

        public static uint StackStart = 0x7fffffff;
        public static uint StackSize = 2 << 16; // 64kb stack page
        public static byte[] StackPage = new byte[2 << 16];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetPage(uint location)
        {
            unchecked
            { 
                if ((location & 0xffff0000) == 0x7fff0000)
                {
                    // stack
                    return StackPage;
                }

                uint pid = location >> 12;

                if (pid == _cpid)
                {
                    return _cp;
                }
                
                byte[] page = Pages[pid];

                _cpid = pid;

                if (page == null)
                    return _cp = Pages[pid] = new byte[(2 << 12)];

                return _cp = page;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(uint location)
        {
            byte[] page = GetPage(location);
            return page[location & 0xFFF];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(uint location, byte v)
        {
            byte[] page = GetPage(location);
            page[location & 0xFFF] = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadWord(uint location)
        {
            byte[] page = GetPage(location);

            location &= 0xFFF;

            return page[location] |
                ((uint)page[location + 1] << 8) |
                ((uint)page[location + 2] << 16) |
                ((uint)page[location + 3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteWord(uint word, uint location)
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
