using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nimp
{
    public class Utilities
    {
        public static void DumpInstruction(uint i)
        {
            var op = (int)GetOpcode(i);
            var func = (int)GetFunc(i);

            var op_str = "";
            var func_str = "";

            if (Enum.IsDefined(typeof(Opcodes), op))
                op_str = string.Format("({0})", Enum.GetName(typeof(Opcodes), op));

            if (op == 0x00 && Enum.IsDefined(typeof(AluFuncs), func))
                func_str = string.Format("({0})", Enum.GetName(typeof(AluFuncs), func));

            Console.WriteLine("opcode: 0x{0:X2}{5}, $s: ${1:00}, $t: ${2:00}, $d: ${3:00}, func: 0x{4:X2}{6}",
                GetOpcode(i),
                GetS(i),
                GetT(i),
                GetD(i),
                GetFunc(i),
                op_str,
                func_str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // just to be safe
        public static long GetOpcode(uint word)
        {
            return (word & (0x3F << 26)) >> 26;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetS(uint word)
        {
            return (word & (0x1F << 21)) >> 21;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetT(uint word)
        {
            return (word & (0x1F << 16)) >> 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetD(uint word)
        {
            return (word & (0x1F << 11)) >> 11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetShift(uint word)
        {
            return (int)((word & (0x1F << 6)) >> 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetI(uint word)
        {
            return (int)(word & (0xFFFF));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetFunc(uint word)
        {
            return word & (0x3F);
        }
    }
}
