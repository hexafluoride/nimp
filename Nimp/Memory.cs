using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public unsafe static class Memory
    {
        public static byte*[] Pages = new byte*[1 << 20];
        static byte* _cp; // cached page
        static uint _cpid = uint.MaxValue;

        public static uint StackStart = 0x7fffffff;
        public static uint StackSize = 1 << 16; // 64kb stack page
        public static byte* StackPage;

        public static void Init()
        {
            Memory.StackPage = (byte*)Marshal.AllocHGlobal(1 << 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* GetPage(uint location)
        {
            unchecked
            { 
                if ((location & 0xffff0000) == 0x7fff0000)
                {
                    // stack
                    return StackPage;
                }

                uint pid = location >> 12; // calculate page id

                if (pid == _cpid)
                {
                    return _cp; // return cached page
                }

                if(_cpid != 0xffffffff)
                {
                    // commit cached page
                    Pages[_cpid] = _cp;
                }
                
                byte* page = Pages[pid];
                _cpid = pid; // write to cache

                if (page == null)
                    return _cp = Pages[pid] = (byte*)Marshal.AllocHGlobal(1 << 12);
                return _cp = page;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(uint location)
        {
            byte* page = GetPage(location);
            return page[location & 0xFFF];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(uint location, byte v)
        {
            byte* page = GetPage(location);
            page[location & 0xFFF] = v;
        }

        // TODO: Handle unaligned reads across pages

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadWord(uint location)
        {
            byte* page = GetPage(location);

            location &= 0xFFF;

            return page[location] |
                ((uint)page[location + 1] << 8) |
                ((uint)page[location + 2] << 16) |
                ((uint)page[location + 3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteWord(uint word, uint location)
        {
            byte* page = GetPage(location);

            location &= 0xFFF;

            page[location] = (byte)(word & 0xFF);
            page[location + 1] = (byte)((word & 0xFF00) >> 8);
            page[location + 2] = (byte)((word & 0xFF0000) >> 16);
            page[location + 3] = (byte)((word & 0xFF000000) >> 24);
        }
    }
}
