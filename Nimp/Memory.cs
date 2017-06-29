using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nimp
{
    public static class Memory
    {
        public static byte[][] Pages = new byte[1 << 20][];
        static byte[] _cp; // cached page
        static uint _cpid = uint.MaxValue;

        public static uint StackStart = 0x7fffffff;
        public static uint StackSize = 1 << 16; // 64kb stack page
        public static byte[] StackPage = new byte[1 << 16];

        public static uint[][][] InstructionCache = new uint[1 << 20][][];

        public static ConcurrentQueue<uint> InstructionCacheQueue = new ConcurrentQueue<uint>();
        static ManualResetEvent _cache_added = new ManualResetEvent(false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode()
        {
            //var i_page = InstructionCache[State.PC >> 12];

            //if (i_page != null)
            //{
            //    uint location = (State.PC & 0xFFC) >> 2;
            //    var i_entry = i_page[location];

            //    State._opcode = i_entry[0];
            //    State._s = i_entry[1];
            //    State._t = i_entry[2];
            //    State._d = i_entry[3];
            //    State._shift = unchecked((int)i_entry[4]);
            //    State._i = unchecked((int)i_entry[5]);
            //    State._func = i_entry[6];
            //    State._instruction = i_entry[7];
            //}
            //else
            //{


            //    InstructionCacheQueue.Enqueue(State.PC);
            //    _cache_added.Set();
            //}
        }

        public static void CacheThread()
        {
            //while(true)
            //{
            //    uint pc = 0;

            //    if (InstructionCacheQueue.TryDequeue(out pc))
            //        CacheInstructions(pc >> 12);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CacheInstructions(uint page_id)
        {
            uint[][] i_page = InstructionCache[page_id];

            if (i_page != null)
                return;

            i_page = new uint[1 << 10][];
            
            for (uint i = 0; i < (1 << 10); i++)
            {
                i_page[i] = new uint[8];
                uint word = ReadWord(unchecked((uint)((page_id << 12) | (i << 2))));

                i_page[i][0] = unchecked((uint)(word & (0x3F << 26)) >> 26);
                i_page[i][1] = (word & (0x1F << 21)) >> 21;
                i_page[i][2] = (word & (0x1F << 16)) >> 16;
                i_page[i][3] = (word & (0x1F << 11)) >> 11;
                i_page[i][4] = ((word & (0x1F << 6)) >> 6);
                i_page[i][5] = (word & (0xFFFF));
                i_page[i][6] = word & (0x3F);
                i_page[i][7] = word;
            }

            InstructionCache[page_id] = i_page;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidateInstructionCache(uint page_id)
        {
            if (InstructionCache[page_id] == null)
                return;

            lock (InstructionCache[page_id])
            {
                InstructionCache[page_id] = null;
            }
        }

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

                uint pid = location >> 12; // calculate page id

                if (pid == _cpid)
                {
                    return _cp; // return cached page
                }

                if(_cpid != 0xffffffff)
                    Pages[_cpid] = _cp; // commit back

                byte[] page = Pages[pid];
                _cpid = pid; // write to cache

                if (page == null)
                    return _cp = Pages[pid] = new byte[(1 << 12)];
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

            InvalidateInstructionCache(location >> 12);
        }

        // TODO: Handle unaligned reads across pages

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadWord(uint location)
        {
            byte[] page = GetPage(location);

            uint loc_o = location;
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
            
            InvalidateInstructionCache(location >> 12);
        }
    }
}
